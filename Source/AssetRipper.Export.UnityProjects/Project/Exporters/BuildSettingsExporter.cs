using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.SourceGenerated.Classes.ClassID_141;

namespace AssetRipper.Export.UnityProjects.Project.Exporters
{
	public class BuildSettingsExporter : YamlExporterBase
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, TemporaryAssetCollection temporaryFile, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			exportCollection = asset switch
			{
				IBuildSettings settings => new BuildSettingsExportCollection(this, temporaryFile, settings),
				_ => null,
			};
			return exportCollection is not null;
		}
	}
}
