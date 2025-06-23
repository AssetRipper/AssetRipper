using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_189;
using AssetRipper.SourceGenerated.Classes.ClassID_43;

namespace AssetRipper.Export.UnityProjects.Project;

public sealed class YamlStreamedAssetExporter : YamlExporterBase
{
	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		if (asset is IMesh or IImageTexture)
		{
			exportCollection = new YamlStreamedAssetExportCollection(this, asset);
			return true;
		}
		else
		{
			exportCollection = null;
			return false;
		}
	}
}
