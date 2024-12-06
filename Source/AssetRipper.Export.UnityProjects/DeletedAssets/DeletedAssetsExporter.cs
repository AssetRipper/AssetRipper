using AssetRipper.Assets;
using AssetRipper.Processing;

namespace AssetRipper.Export.UnityProjects.DeletedAssets;

public sealed class DeletedAssetsExporter : IAssetExporter
{
	AssetType IAssetExporter.ToExportType(IUnityObjectBase asset)
	{
		throw new NotSupportedException();
	}

	bool IAssetExporter.ToUnknownExportType(Type type, out AssetType assetType)
	{
		assetType = default;
		return false;
	}

	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		if (asset.MainAsset is DeletedAssetsInformation infoAsset)
		{
			exportCollection = new DeletedAssetsExportCollection(infoAsset);
			return true;
		}
		else
		{
			exportCollection = null;
			return false;
		}
	}
}
