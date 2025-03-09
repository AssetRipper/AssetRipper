using AssetRipper.Assets;

namespace AssetRipper.Export.PrimaryContent;

public interface IContentExtractor
{
	bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out ExportCollectionBase? exportCollection);
	bool Export(IUnityObjectBase asset, string filePath, FileSystem fileSystem) => Export([asset], filePath, fileSystem);
	bool Export(IEnumerable<IUnityObjectBase> assets, string filePath, FileSystem fileSystem) => throw new NotSupportedException();
}
