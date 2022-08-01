using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project.Collections;

namespace AssetRipper.Library.Exporters.Terrains
{
	public sealed class TerrainObjExportCollection : AssetExportCollection
	{
		public TerrainObjExportCollection(TerrainObjExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset) => "obj";
	}
}
