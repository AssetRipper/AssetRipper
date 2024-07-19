using AssetRipper.Assets;

namespace AssetRipper.Export.PrimaryContent.Models;

public sealed class GlbExportCollection : SingleExportCollection<IUnityObjectBase>
{
	public GlbExportCollection(IContentExtractor assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
	{
	}

	protected override string ExportExtension => "glb";
}
