using AssetRipper.Assets;

namespace AssetRipper.Export.PrimaryContent;

public interface IContentExtractor
{
	bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out ExportCollectionBase? exportCollection);
	bool Export(IUnityObjectBase asset, string filePath) => Export([asset], filePath);
	bool Export(IEnumerable<IUnityObjectBase> assets, string filePath) => throw new NotSupportedException();
}
