using AssetRipper.Processing.Prefabs;

namespace AssetRipper.Export.PrimaryContent.Models;

public sealed class GlbPrefabModelExportCollection : MultipleExportCollection<PrefabHierarchyObject>
{
	public GlbPrefabModelExportCollection(GlbModelExporter assetExporter, PrefabHierarchyObject asset) : base(assetExporter, asset)
	{
		AddAssets(asset.Assets);
	}

	protected override string ExportExtension => "glb";
}
