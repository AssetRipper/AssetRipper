using AssetRipper.AssemblyDumper.Types;
using AssetRipper.Assets;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass010_InitializeInterfacesAndFactories
{
	public static void DoPass()
	{
		ITypeDefOrRef unityObjectBaseReference = SharedState.Instance.Importer.ImportType<IUnityObjectBase>();
		ITypeDefOrRef unityAssetBaseReference = SharedState.Instance.Importer.ImportType<IUnityAssetBase>();
		foreach (ClassGroup group in SharedState.Instance.ClassGroups.Values)
		{
			group.InitializeInterface(unityObjectBaseReference);
		}
		foreach (SubclassGroup group in SharedState.Instance.SubclassGroups.Values)
		{
			group.InitializeInterface(unityAssetBaseReference);
		}
	}

	private static void InitializeInterface(this ClassGroupBase group, ITypeDefOrRef baseInterfaceReference)
	{
		group.Interface.Namespace = group.Namespace;
		group.Interface.Name = $"I{group.Name}";
		group.Interface.AddInterfaceImplementation(baseInterfaceReference);
		SharedState.Instance.TypesToGroups.Add(group.Interface, group);
		foreach (GeneratedClassInstance instance in group.Instances)
		{
			instance.Type.AddInterfaceImplementation(group.Interface);
		}
	}
}
