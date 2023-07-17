using AssetRipper.Assets;

namespace AssetRipper.Export.UnityProjects.Meshes
{
	public sealed class GlbExportCollection : AssetExportCollection<IUnityObjectBase>
	{
		public GlbExportCollection(IAssetExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset) => "glb";
	}
}
