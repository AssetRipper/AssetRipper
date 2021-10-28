using AssetRipper.Core.Classes;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;

namespace AssetRipper.Core.Project.Exporters
{
	public class BuildSettingsExporter : YamlExporterBase
	{
		public override bool IsHandle(UnityObjectBase asset)
		{
			return asset is IBuildSettings;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObjectBase asset)
		{
			return new BuildSettingsExportCollection(this, virtualFile, asset);
		}
	}
}
