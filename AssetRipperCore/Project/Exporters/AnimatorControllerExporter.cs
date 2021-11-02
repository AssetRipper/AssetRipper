using AssetRipper.Core.Classes.AnimatorController;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;

namespace AssetRipper.Core.Project.Exporters
{
	public class AnimatorControllerExporter : YamlExporterBase
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is AnimatorController;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new AnimatorControllerExportCollection(this, virtualFile, asset);
		}
	}
}
