using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.Export.UnityProjects.Project.Exporters;

namespace AssetRipper.Export.UnityProjects.Meshes
{
	public sealed class GlbExportCollection : AssetExportCollection
	{
		public GlbExportCollection(IAssetExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset) => "glb";
	}
}
