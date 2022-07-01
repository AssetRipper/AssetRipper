using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project.Collections;

namespace AssetRipper.Library.Exporters.Meshes
{
	public sealed class GlbMeshExportCollection : AssetExportCollection
	{
		public GlbMeshExportCollection(GlbMeshExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset) => "glb";
	}
}
