using AssetRipper.Core.Classes;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;

namespace AssetRipper.Core.Project.Exporters
{
	public class ManagerAssetExporter : YamlExporterBase
	{
		public override bool IsHandle(UnityObjectBase asset)
		{
			return asset is IGlobalGameManager && asset is not IBuildSettings;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObjectBase asset)
		{
			return new ManagerExportCollection(this, asset);
		}
	}
}
