using AssetRipper.Assets;
using AssetRipper.Processing;

namespace AssetRipper.Export.PrimaryContent.DeletedAssets;

public sealed class DeletedAssetsExporter : IContentExtractor
{
	public static DeletedAssetsExporter Instance { get; } = new();
	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out ExportCollectionBase? exportCollection)
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
