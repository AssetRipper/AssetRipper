using AssetRipper.Core.Classes.AnimatorController;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;

namespace AssetRipper.Core.Project.Exporters
{
	public class AnimatorControllerExporter : YamlExporterBase
	{
		public override bool IsHandle(UnityObjectBase asset)
		{
			return asset is AnimatorController;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObjectBase asset)
		{
			return new AnimatorControllerExportCollection(this, virtualFile, asset);
		}
	}
}
