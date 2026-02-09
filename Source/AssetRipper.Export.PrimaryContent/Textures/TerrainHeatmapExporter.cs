using AssetRipper.Assets;
using AssetRipper.Export.Configuration;
using AssetRipper.Export.Modules.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_156;

namespace AssetRipper.Export.PrimaryContent.Textures;

public sealed class TerrainHeatmapExporter : IContentExtractor
{
	public ImageExportFormat ImageFormat { get; }
	public TerrainHeatmapExporter(ImageExportFormat imageFormat)
	{
		ImageFormat = imageFormat;
	}

	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out ExportCollectionBase? exportCollection)
	{
		if (asset is ITerrainData terrainData)
		{
			exportCollection = new TerrainHeatmapExportCollection(this, terrainData);
			return true;
		}
		else
		{
			exportCollection = null;
			return false;
		}
	}

	public bool Export(IUnityObjectBase asset, string path, FileSystem fileSystem)
	{
		ITerrainData terrain = (ITerrainData)asset;
		DirectBitmap bitmap = TerrainHeatmap.GetBitmap(terrain);
		using Stream stream = fileSystem.File.Create(path);
		bitmap.Save(stream, ImageFormat);
		return true;
	}
}
