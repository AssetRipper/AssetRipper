using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.SourceGenerated.Classes.ClassID_91;

namespace AssetRipper.Core.Project.Exporters
{
	internal class AnimatorControllerExporter : YamlExporterBase
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is IAnimatorController;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new AnimatorControllerExportCollection(this, virtualFile, asset);
		}
	}
}
