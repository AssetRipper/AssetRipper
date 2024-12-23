using AssetRipper.Assets;

namespace AssetRipper.Export.PrimaryContent;

public sealed class EmptyContentExtractor : IContentExtractor
{
	public static EmptyContentExtractor Instance { get; } = new();
	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out ExportCollectionBase? exportCollection)
	{
		exportCollection = EmptyExportCollection.Instance;
		return true;
	}
}
