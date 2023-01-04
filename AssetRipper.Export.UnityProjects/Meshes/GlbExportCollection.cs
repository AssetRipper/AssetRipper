using AssetRipper.Assets;
using AssetRipper.Import.Project.Collections;
using AssetRipper.Import.Project.Exporters;

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
