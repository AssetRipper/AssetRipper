using AssetRipper.AssemblyDumper.InjectedTypes;
using AssetRipper.AssemblyDumper.Types;
using AssetRipper.Assets;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass510_FixComponentPairWalking
{
	public static void DoPass()
	{
		TypeDefinition walkingHelperType = SharedState.Instance.InjectHelperType(typeof(WalkingHelper));
		MethodDefinition releaseMethod = walkingHelperType.Methods.Single(m => m.Name == nameof(WalkingHelper.WalkComponentPairRelease));
		MethodDefinition editorMethod = walkingHelperType.Methods.Single(m => m.Name == nameof(WalkingHelper.WalkComponentPairEditor));
		MethodDefinition standardMethod = walkingHelperType.Methods.Single(m => m.Name == nameof(WalkingHelper.WalkComponentPairStandard));
		SubclassGroup componentPairGroup = SharedState.Instance.SubclassGroups["ComponentPair"];
		foreach (GeneratedClassInstance instance in componentPairGroup.Instances)
		{
			ClassProperty classId = instance.Properties.First(p => p.Name == "ClassID");
			ClassProperty component = instance.Properties.First(p => p.Name == "Component");
			if (!classId.IsPresent)
			{
				continue;
			}

			instance.Type.GetMethodByName(nameof(IUnityAssetBase.WalkRelease)).FixWalkMethod(releaseMethod, classId.BackingField, component.BackingField!);
			instance.Type.GetMethodByName(nameof(IUnityAssetBase.WalkEditor)).FixWalkMethod(editorMethod, classId.BackingField, component.BackingField!);
			instance.Type.GetMethodByName(nameof(IUnityAssetBase.WalkStandard)).FixWalkMethod(standardMethod, classId.BackingField, component.BackingField!);
		}
	}

	private static void FixWalkMethod(this MethodDefinition method, MethodDefinition injectedMethod, FieldDefinition classIDField, FieldDefinition componentField)
	{
		CilInstructionCollection instructions = method.GetInstructions();
		instructions.Clear();
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldfld, classIDField);
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldfld, componentField);
		instructions.Add(CilOpCodes.Ldarg_1);
		instructions.Add(CilOpCodes.Call, injectedMethod.MakeGenericInstanceMethod(componentField.Signature!.FieldType));
		instructions.Add(CilOpCodes.Ret);
	}
}
