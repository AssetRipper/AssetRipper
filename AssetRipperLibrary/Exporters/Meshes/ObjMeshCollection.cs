using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Structure.Collections;

namespace AssetRipper.Library.Exporters.Meshes
{
	internal class ObjMeshCollection : AssetExportCollection
	{
		public ObjMeshCollection(IAssetExporter assetExporter, Mesh asset) : base(assetExporter, asset) { }

		protected override string GetExportExtension(Object asset)
		{
			return "obj";
		}
	}
}