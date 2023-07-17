using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Project;

namespace AssetRipper.Export.UnityProjects.Models
{
	public sealed class GlbSceneModelExportCollection : SceneExportCollection
	{
		public GlbSceneModelExportCollection(GlbModelExporter assetExporter, SceneDefinition scene) : base(assetExporter, scene)
		{
		}

		protected override bool ExportScene(IExportContainer container, string folderPath, string filePath, string sceneName)
		{
			return ((GlbModelExporter)AssetExporter).ExportModel(Assets, filePath, true);
		}

		public override string ExportExtension => "glb";
	}
}
