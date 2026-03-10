using AsmResolver.DotNet;
using AssetRipper.CIL;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Processing.Assemblies;

/// <summary>
/// Publicizes only Assembly-CSharp style assemblies so decompiled user scripts stay accessible without touching package assemblies.
/// </summary>
public sealed class AssemblyCSharpPublicizingProcessor : IAssetProcessor
{
	public void Process(GameData gameData) => Process(gameData.AssemblyManager);

	private static void Process(IAssemblyManager manager)
	{
		ModuleDefinition? mscorlib = manager.Mscorlib?.ManifestModule;
		if (mscorlib is null || !mscorlib.TryGetTopLevelType("System", "NonSerializedAttribute", out TypeDefinition? nonSerializedAttributeType))
		{
			return;
		}

		MethodDefinition? nonSerializedAttributeConstructor = nonSerializedAttributeType.Methods.FirstOrDefault(static method => method.IsConstructor && method.Parameters.Count == 0);
		if (nonSerializedAttributeConstructor is null)
		{
			return;
		}

		manager.ClearStreamCache();

		foreach (AssemblyDefinition assembly in manager.GetAssemblies().Where(IsAssemblyCSharpAssembly))
		{
			foreach (ModuleDefinition module in assembly.Modules)
			{
				ICustomAttributeType importedNonSerializedAttributeConstructor = (ICustomAttributeType)module.DefaultImporter.ImportMethod(nonSerializedAttributeConstructor);

				foreach (TypeDefinition type in module.GetAllTypes())
				{
					if (type.IsModuleType || IsDiscordType(type))
					{
						continue;
					}

					SetPublic(type);

					foreach (MethodDefinition method in type.Methods)
					{
						if (PublicizingSupport.ShouldSkipMethodPublicizing(method))
						{
							continue;
						}

						method.IsPublic = true;
					}

					foreach (FieldDefinition field in type.Fields)
					{
						PublicizingSupport.DeduplicateNonSerializedAttributes(field);

						if (field.IsCompilerGenerated() || field.IsPublic)
						{
							continue;
						}

						field.IsPublic = true;

						if (field.IsStatic)
						{
							continue;
						}
						if (!field.CustomAttributes.Any(CustomAttributeExtensions.IsSerializeField)
							&& !field.CustomAttributes.Any(CustomAttributeExtensions.IsNonSerializedAttribute))
						{
							field.CustomAttributes.Add(new CustomAttribute(importedNonSerializedAttributeConstructor));
						}
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

	private static bool IsAssemblyCSharpAssembly(AssemblyDefinition assembly)
	{
		return NormalizeAssemblyName(assembly.Name).StartsWith("assemblycsharp", StringComparison.Ordinal);
	}

	private static string NormalizeAssemblyName(string? name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return string.Empty;
		}

		return new string(name.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
	}

	private static bool IsDiscordType(TypeDefinition type)
	{
		string fullName = type.FullName ?? string.Empty;
		return fullName.IndexOf("Discord", StringComparison.OrdinalIgnoreCase) >= 0;
	}
}
