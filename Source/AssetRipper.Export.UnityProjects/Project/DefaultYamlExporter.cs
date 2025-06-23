using AssetRipper.Assets;

namespace AssetRipper.Export.UnityProjects.Project;

public class DefaultYamlExporter : YamlExporterBase
{
	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		exportCollection = new AssetExportCollection<IUnityObjectBase>(this, asset);
		return true;
	}
}
