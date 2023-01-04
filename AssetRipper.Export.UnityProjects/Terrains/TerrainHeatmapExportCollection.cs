using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Import.Project.Collections;

namespace AssetRipper.Export.UnityProjects.Terrains
{
	public sealed class TerrainHeatmapExportCollection : AssetExportCollection
	{
		public TerrainHeatmapExportCollection(TerrainHeatmapExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			return ((TerrainHeatmapExporter)AssetExporter).ImageFormat.GetFileExtension();
		}
	}
}
