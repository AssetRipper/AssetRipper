using AssetRipper.Assets;

namespace AssetRipper.Export.UnityProjects;

public interface IAssetExporter
{
	bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection);

	bool Export(IExportContainer container, IUnityObjectBase asset, string path, FileSystem fileSystem)
	{
		Export(container, asset, path, fileSystem, null);
		return true;
	}
	void Export(IExportContainer container, IUnityObjectBase asset, string path, FileSystem fileSystem, Action<IExportContainer, IUnityObjectBase, string, FileSystem>? callback)
	{
		throw new NotSupportedException();
	}
	bool Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path, FileSystem fileSystem)
	{
		Export(container, assets, path, fileSystem, null);
		return true;
	}
	void Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path, FileSystem fileSystem, Action<IExportContainer, IUnityObjectBase, string, FileSystem>? callback)
	{
		throw new NotSupportedException();
	}

	AssetType ToExportType(IUnityObjectBase asset);
	bool ToUnknownExportType(Type type, out AssetType assetType);
}
