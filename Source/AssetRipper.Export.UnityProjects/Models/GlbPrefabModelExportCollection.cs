using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Processing;

namespace AssetRipper.Export.UnityProjects.Models
{
	public sealed class GlbPrefabModelExportCollection : PrefabExportCollection
	{
		public GlbPrefabModelExportCollection(GlbModelExporter assetExporter, PrefabHierarchyObject asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset) => "glb";
	}
}
