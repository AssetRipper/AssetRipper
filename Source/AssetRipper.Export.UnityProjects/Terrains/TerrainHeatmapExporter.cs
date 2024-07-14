using AssetRipper.Assets;
using AssetRipper.Export.Modules.Textures;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_156;

namespace AssetRipper.Export.UnityProjects.Terrains
{
	public sealed class TerrainHeatmapExporter : BinaryAssetExporter
	{
		public ImageExportFormat ImageFormat { get; }
		public TerrainHeatmapExporter(LibraryConfiguration configuration)
		{
			ImageFormat = configuration.ExportSettings.ImageExportFormat;
		}

		public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
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

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			ITerrainData terrain = (ITerrainData)asset;
			DirectBitmap bitmap = TerrainHeatmap.GetBitmap(terrain);
			using FileStream stream = File.Create(path);
			bitmap.Save(stream, ImageFormat);
			return true;
		}
	}
}
