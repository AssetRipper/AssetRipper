using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Import.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_156;

namespace AssetRipper.Export.UnityProjects.Terrains
{
	public sealed class TerrainYamlExporter : YamlExporterBase
	{
		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			ITerrainData terrainData = (ITerrainData?)asset.MainAsset ?? throw new NullReferenceException();
			return new TerrainYamlExportCollection(this, terrainData);
		}

		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset.MainAsset is ITerrainData;
		}
	}
}
