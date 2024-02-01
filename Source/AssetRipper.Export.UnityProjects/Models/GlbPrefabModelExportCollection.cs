using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Project;

namespace AssetRipper.Export.UnityProjects.Models
{
	public sealed class GlbPrefabModelExportCollection : PrefabExportCollection
	{
		public GlbPrefabModelExportCollection(GlbModelExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset) => "glb";
	}
}
