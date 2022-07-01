using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Library.Configuration;

namespace AssetRipper.Library.Exporters.Terrains
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
