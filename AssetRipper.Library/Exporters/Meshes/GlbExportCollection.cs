using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;

namespace AssetRipper.Library.Exporters.Meshes
{
	public sealed class GlbExportCollection : AssetExportCollection
	{
		public GlbExportCollection(IAssetExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset) => "glb";
	}
}
