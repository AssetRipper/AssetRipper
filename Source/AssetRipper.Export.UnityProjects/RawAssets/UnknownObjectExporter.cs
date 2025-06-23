using AssetRipper.Assets;
using AssetRipper.Import.AssetCreation;

namespace AssetRipper.Export.UnityProjects.RawAssets;

public sealed class UnknownObjectExporter : IAssetExporter
{
	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		if (asset is UnknownObject @object)
		{
			exportCollection = new UnknownExportCollection(this, @object);
			return true;
		}
		else
		{
			exportCollection = null;
			return false;
		}
	}

	public bool Export(IExportContainer container, IUnityObjectBase asset, string path, FileSystem fileSystem)
	{
		fileSystem.File.WriteAllBytes(path, ((UnknownObject)asset).RawData);
		return true;
	}

	public void Export(IExportContainer container, IUnityObjectBase asset, string path, FileSystem fileSystem, Action<IExportContainer, IUnityObjectBase, string, FileSystem>? callback)
	{
		if (Export(container, asset, path, fileSystem))
		{
			callback?.Invoke(container, asset, path, fileSystem);
		}
	}

	public bool Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path, FileSystem fileSystem)
	{
		bool success = true;
		foreach (IUnityObjectBase asset in assets)
		{
			success &= Export(container, asset, path, fileSystem);
		}
		return success;
	}

	public void Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path, FileSystem fileSystem, Action<IExportContainer, IUnityObjectBase, string, FileSystem>? callback)
	{
		foreach (IUnityObjectBase asset in assets)
		{
			Export(container, asset, path, fileSystem, callback);
		}
	}

	public AssetType ToExportType(IUnityObjectBase asset)
	{
		return AssetType.Meta;
	}

	public bool ToUnknownExportType(Type type, out AssetType assetType)
	{
		assetType = AssetType.Meta;
		return true;
	}
}
