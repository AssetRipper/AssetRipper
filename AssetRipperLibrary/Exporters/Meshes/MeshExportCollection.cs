using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Structure.Collections;

namespace AssetRipper.Library.Exporters.Meshes
{
	internal class MeshExportCollection : AssetExportCollection
	{
		private string fileExtension;
		public MeshExportCollection(IAssetExporter assetExporter, Mesh asset, string extention) : base(assetExporter, asset)
		{
			fileExtension = extention;
		}

		protected override string GetExportExtension(Object asset)
		{
			return fileExtension;
		}
	}
}