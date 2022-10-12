using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_28;

namespace AssetRipper.Library.Exporters.Terrains
{
	public sealed class TerrainYamlExporter : YamlExporterBase
	{
		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			ITerrainData terrainData = asset switch
			{
				ITexture2D texture => texture.TerrainData ?? throw new NullReferenceException(),
				_ => (ITerrainData)asset,
			};
			return new TerrainYamlExportCollection(this, terrainData);
		}

		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is ITerrainData || (asset is ITexture2D texture && texture.TerrainData is not null);
		}
	}
}
