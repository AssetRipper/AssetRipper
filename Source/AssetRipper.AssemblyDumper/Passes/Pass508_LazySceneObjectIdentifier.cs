using AssetRipper.Assets;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass508_LazySceneObjectIdentifier
{
	public const string TargetObjectName = "TargetObjectReference";
	public const string TargetPrefabName = "TargetPrefabReference";
	public static void DoPass()
	{
		TypeSignature unityObjectBase = SharedState.Instance.Importer.ImportType<IUnityObjectBase>().ToTypeSignature();

		SubclassGroup group = SharedState.Instance.SubclassGroups["SceneObjectIdentifier"];
		PropertyInjector.InjectFullProperty(group, unityObjectBase, TargetObjectName, true);
		PropertyInjector.InjectFullProperty(group, unityObjectBase, TargetPrefabName, true);
	}
}
