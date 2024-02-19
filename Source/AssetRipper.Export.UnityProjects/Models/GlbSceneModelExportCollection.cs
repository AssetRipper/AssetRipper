using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Processing;

namespace AssetRipper.Export.UnityProjects.Models
{
	public sealed class GlbSceneModelExportCollection : SceneExportCollection
	{
		public GlbSceneModelExportCollection(GlbModelExporter assetExporter, SceneHierarchyObject asset) : base(assetExporter, asset)
		{
		}

		protected override bool ExportScene(IExportContainer container, string folderPath, string filePath, string sceneName)
		{
			return GlbModelExporter.ExportModel(ExportableAssets, filePath, true);
		}

		public override string ExportExtension => "glb";
	}
}
