using AssetRipper.Assets.Metadata;

namespace AssetRipper.AssemblyDumper.Passes;

public static class Pass061_AddConstructors
{
	private const MethodAttributes PublicInstanceConstructorAttributes =
		MethodAttributes.Public |
		MethodAttributes.HideBySig |
		MethodAttributes.SpecialName |
		MethodAttributes.RuntimeSpecialName;
	private readonly static HashSet<GeneratedClassInstance> processed = new HashSet<GeneratedClassInstance>();
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	private static ITypeDefOrRef AssetInfoRef;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	public static void DoPass()
	{
		AssetInfoRef = SharedState.Instance.Importer.ImportType<AssetInfo>();
		IEnumerable<ClassGroupBase> groups = SharedState.Instance.ClassGroups.Values.Select(g => (ClassGroupBase)g)
			.Union(SharedState.Instance.SubclassGroups.Values.Select(g => (ClassGroupBase)g));
		foreach (ClassGroupBase group in groups)
		{
			foreach (GeneratedClassInstance instance in group.Instances)
			{
				if (processed.Contains(instance))
				{
					continue;
				}

				AddConstructor(instance);
			}
		}
		processed.Clear();
	}

	private static void AddConstructor(GeneratedClassInstance instance)
	{
		if (instance.Base is not null && !processed.Contains(instance.Base))
		{
			AddConstructor(instance.Base);
		}

		TypeDefinition type = instance.Type;
		if (instance.Group is ClassGroup)
		{
			type.AddAssetInfoConstructor();
		}
		else
		{
			type.AddEmptyDefaultConstructor();
		}
		processed.Add(instance);
	}

	private static MethodDefinition AddAssetInfoConstructor(this TypeDefinition typeDefinition)
	{
		return AddSingleParameterConstructor(typeDefinition, AssetInfoRef, "info");
	}

	private static MethodDefinition AddEmptyDefaultConstructor(this TypeDefinition typeDefinition)
	{
		return typeDefinition.AddMethod(
			".ctor",
			PublicInstanceConstructorAttributes,
			SharedState.Instance.Importer.Void
		);
	}

	private static MethodDefinition AddSingleParameterConstructor(this TypeDefinition typeDefinition, ITypeDefOrRef parameterType, string parameterName)
	{
		MethodDefinition? constructor = typeDefinition.AddMethod(
			".ctor",
			PublicInstanceConstructorAttributes,
			SharedState.Instance.Importer.Void
		);
		constructor.AddParameter(parameterType.ToTypeSignature(), parameterName);
		return constructor;
	}
}