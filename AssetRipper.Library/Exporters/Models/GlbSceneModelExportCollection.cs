using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;

namespace AssetRipper.Library.Exporters.Models
{
	public sealed class GlbSceneModelExportCollection : SceneExportCollection
	{
		public GlbSceneModelExportCollection(GlbModelExporter assetExporter, ISerializedFile scene) : base(assetExporter, scene)
		{
		}

		protected override bool ExportScene(IProjectAssetContainer container, string folderPath, string filePath, string sceneName)
		{
			return ((GlbModelExporter)AssetExporter).ExportModel(Assets, filePath, true);
		}

		public override string ExportExtension => "glb";
	}
}
