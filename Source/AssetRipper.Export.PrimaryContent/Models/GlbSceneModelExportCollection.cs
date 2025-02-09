using AssetRipper.Processing.Prefabs;

namespace AssetRipper.Export.PrimaryContent.Models;

public sealed class GlbSceneModelExportCollection : MultipleExportCollection<SceneHierarchyObject>
{
	public GlbSceneModelExportCollection(GlbModelExporter assetExporter, SceneHierarchyObject asset) : base(assetExporter, asset)
	{
		AddAssets(asset.Assets);
	}

	protected override string ExportExtension => "glb";
}
