using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.SourceGenerated.Classes.ClassID_156;

namespace AssetRipper.Export.UnityProjects.Terrains;

public sealed class TerrainYamlExporter : YamlExporterBase
{
	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		exportCollection = asset.MainAsset switch
		{
			ITerrainData terrainData => new TerrainYamlExportCollection(this, terrainData),
			_ => null,
		};
		return exportCollection is not null;
	}
}
