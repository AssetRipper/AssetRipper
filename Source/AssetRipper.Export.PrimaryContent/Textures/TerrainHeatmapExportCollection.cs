using AssetRipper.Export.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_156;

namespace AssetRipper.Export.PrimaryContent.Textures;

public sealed class TerrainHeatmapExportCollection : SingleExportCollection<ITerrainData>
{
	public TerrainHeatmapExportCollection(TerrainHeatmapExporter assetExporter, ITerrainData asset) : base(assetExporter, asset)
	{
	}

	protected override string ExportExtension => ((TerrainHeatmapExporter)ContentExtractor).ImageFormat.GetFileExtension();
}
