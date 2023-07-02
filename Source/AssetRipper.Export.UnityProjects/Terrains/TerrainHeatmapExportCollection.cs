using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.SourceGenerated.Classes.ClassID_156;

namespace AssetRipper.Export.UnityProjects.Terrains
{
	public sealed class TerrainHeatmapExportCollection : AssetExportCollection<ITerrainData>
	{
		public TerrainHeatmapExportCollection(TerrainHeatmapExporter assetExporter, ITerrainData asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			return ((TerrainHeatmapExporter)AssetExporter).ImageFormat.GetFileExtension();
		}
	}
}
