using AssetRipper.Assets;

namespace AssetRipper.Export.PrimaryContent;

public interface IContentExtractor
{
	bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out ExportCollectionBase? exportCollection);
	bool Export(IUnityObjectBase asset, string filePath);
}
