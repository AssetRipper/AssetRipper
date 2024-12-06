using AssetRipper.Assets;
using AssetRipper.Processing;

namespace AssetRipper.Export.PrimaryContent.DeletedAssets;

public sealed class DeletedAssetsExportCollection(DeletedAssetsInformation Asset) : ExportCollectionBase
{
	public override bool Exportable => false;

	public override IEnumerable<IUnityObjectBase> Assets => Asset.DeletedAssets.Append(Asset);

	public override string Name => nameof(DeletedAssetsExportCollection);

	public override IContentExtractor ContentExtractor => DeletedAssetsExporter.Instance;

	public override bool Contains(IUnityObjectBase asset)
	{
		return ReferenceEquals(Asset, asset) || Asset.DeletedAssets.Contains(asset);
	}

	public override bool Export(string projectDirectory, FileSystem fileSystem)
	{
		throw new NotSupportedException();
	}
}
