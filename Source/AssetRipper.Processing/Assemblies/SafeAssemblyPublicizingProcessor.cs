using AsmResolver.DotNet;
using AssetRipper.CIL;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Processing.Assemblies;

/// <summary>
/// This processor makes (nearly) all types, methods, and fields public in the assembly.
/// It handles edge cases like determing if something should be publicized and
/// adding [NonSerialized] to newly public instance fields.
/// </summary>
public sealed class SafeAssemblyPublicizingProcessor : IAssetProcessor
{
	public void Process(GameData gameData) => Process(gameData.AssemblyManager);
	private static void Process(IAssemblyManager manager)
	{
		ModuleDefinition? mscorlib = manager.Mscorlib?.ManifestModule;
		if (mscorlib is null)
		{
			return;
		}

		if (!mscorlib.TryGetTopLevelType("System", "NonSerializedAttribute", out TypeDefinition? nonSerializedAttributeType))
		{
			return;
		}

		MethodDefinition? nonSerializedAttributeConstructor = nonSerializedAttributeType.Methods.FirstOrDefault(m => m.IsConstructor && m.Parameters.Count == 0);
		if (nonSerializedAttributeConstructor is null)
		{
			return;
		}

		manager.ClearStreamCache();

		foreach (ModuleDefinition module in manager.GetAllModules())
		{
			ICustomAttributeType importedNonSerializedAttributeConstructor = (ICustomAttributeType)module.DefaultImporter.ImportMethod(nonSerializedAttributeConstructor);

			foreach (TypeDefinition type in module.GetAllTypes())
			{
				if (type.IsModuleType)
				{
					continue;
				}

				SetPublic(type);

				// Properties and events are technically always public. It's only their associated methods that might not be public.

				foreach (MethodDefinition method in type.Methods)
				{
					if (method.IsStaticConstructor())
					{
						continue;
					}
					if (method.Name == "Finalize" && method.IsVirtual && method.IsFamily && method.DeclaringType!.MethodImplementations.Any(i => i.Body == method))
					{
						// Finalizers should not be modified.
						continue;
					}
					if (method.DeclaringType!.MethodImplementations.Any(i => i.Body == method))
					{
						// Explicit interface implementations should not be modified.
						continue;
					}
					method.IsPublic = true;
				}

				foreach (FieldDefinition field in type.Fields)
				{
					if (field.IsCompilerGenerated())
					{
						// Backing fields should not be modified.
						continue;
					}
					if (field.IsPublic)
					{
						// Already public.
						continue;
					}

					field.IsPublic = true;

					if (field.IsStatic)
					{
						// Static fields aren't serializable.
						continue;
					}

					if (!field.CustomAttributes.Any(CustomAttributeExtensions.IsSerializeField) &&
						!field.CustomAttributes.Any(CustomAttributeExtensions.IsNonSerializedAttribute))
					{
						// Need to add [NonSerialized] to prevent Unity from serializing this field now that it's public.
						field.CustomAttributes.Add(new CustomAttribute(importedNonSerializedAttributeConstructor));
					}
				}
			}
		}
	}

	private static void SetPublic(TypeDefinition type)
	{
		if (type.IsNested)
		{
			type.IsNestedPublic = true;
		}
		else
		{
			type.IsPublic = true;
		}
	}
}
