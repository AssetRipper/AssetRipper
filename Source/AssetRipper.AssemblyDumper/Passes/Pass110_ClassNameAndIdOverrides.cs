using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.Assets;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass110_ClassNameAndIdOverrides
{
	const MethodAttributes PropertyOverrideAttributes =
		MethodAttributes.Public |
		MethodAttributes.HideBySig |
		MethodAttributes.SpecialName |
		MethodAttributes.ReuseSlot |
		MethodAttributes.Virtual;

	public static void DoPass()
	{
		foreach ((int id, ClassGroup group) in SharedState.Instance.ClassGroups)
		{
			foreach (GeneratedClassInstance instance in group.Instances)
			{
				instance.Type.AddClassNameOverride(instance.Class.OriginalName);
			}
		}
		foreach (GeneratedClassInstance instance in SharedState.Instance.AllGroups.SelectMany(g => g.Instances))
		{
			instance.Type.AddSerializedVersionOverride(GetSerializedVersion(instance));
			instance.Type.AddFlowMappedInYamlOverride(GetFlowMappedInYaml(instance));
		}
	}

	private static void AddClassNameOverride(this TypeDefinition type, string className)
	{
		PropertyDefinition property = type.AddGetterProperty(nameof(UnityObjectBase.ClassName), PropertyOverrideAttributes, SharedState.Instance.Importer.String);
		CilInstructionCollection instructions = property.GetMethod!.CilMethodBody!.Instructions;
		instructions.Add(CilOpCodes.Ldstr, className);
		instructions.Add(CilOpCodes.Ret);
		property.AddDebuggerBrowsableNeverAttribute();
	}

	private static void AddSerializedVersionOverride(this TypeDefinition type, int version)
	{
		PropertyDefinition property = type.AddGetterProperty(nameof(UnityAssetBase.SerializedVersion), PropertyOverrideAttributes, SharedState.Instance.Importer.Int32);
		CilInstructionCollection instructions = property.GetMethod!.CilMethodBody!.Instructions;
		instructions.Add(CilOpCodes.Ldc_I4, version);
		instructions.Add(CilOpCodes.Ret);
		property.AddDebuggerBrowsableNeverAttribute();
	}

	private static void AddFlowMappedInYamlOverride(this TypeDefinition type, bool flowMapped)
	{
		PropertyDefinition property = type.AddGetterProperty(nameof(UnityAssetBase.FlowMappedInYaml), PropertyOverrideAttributes, SharedState.Instance.Importer.Boolean);
		CilInstructionCollection instructions = property.GetMethod!.CilMethodBody!.Instructions;
		instructions.Add(flowMapped ? CilOpCodes.Ldc_I4_1 : CilOpCodes.Ldc_I4_0);
		instructions.Add(CilOpCodes.Ret);
		property.AddDebuggerBrowsableNeverAttribute();
	}

	private static int GetSerializedVersion(GeneratedClassInstance instance)
	{
		if (instance.Class.EditorRootNode is null)
		{
			return instance.Class.ReleaseRootNode?.Version ?? 1;
		}
		else if (instance.Class.ReleaseRootNode is null)
		{
			return instance.Class.EditorRootNode.Version;
		}
		else
		{
			int release = instance.Class.ReleaseRootNode.Version;
			int editor = instance.Class.EditorRootNode.Version;
			return release == editor ? release : throw new("Release and editor serialized versions were different!");
		}
	}

	private static bool GetFlowMappedInYaml(GeneratedClassInstance instance)
	{
		if (instance.Class.EditorRootNode is null)
		{
			return instance.Class.ReleaseRootNode?.MetaFlag.IsTransferUsingFlowMappingStyle() ?? false;
		}
		else if (instance.Class.ReleaseRootNode is null)
		{
			return instance.Class.EditorRootNode.MetaFlag.IsTransferUsingFlowMappingStyle();
		}
		else
		{
			bool release = instance.Class.ReleaseRootNode.MetaFlag.IsTransferUsingFlowMappingStyle();
			bool editor = instance.Class.EditorRootNode.MetaFlag.IsTransferUsingFlowMappingStyle();
			return release == editor ? release : throw new("Release and editor flow mapping were different!");
		}
	}
}
