using AssetRipper.Assets;
using AssetRipper.Export.Modules.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_156;

namespace AssetRipper.Export.PrimaryContent.Textures;

public sealed class TerrainHeatmapExportCollection : SingleExportCollection<ITerrainData>
{
	public TerrainHeatmapExportCollection(TerrainHeatmapExporter assetExporter, ITerrainData asset) : base(assetExporter, asset)
	{
	}

	protected override string GetExportExtension(IUnityObjectBase asset)
	{
		return ((TerrainHeatmapExporter)ContentExtractor).ImageFormat.GetFileExtension();
	}
}
