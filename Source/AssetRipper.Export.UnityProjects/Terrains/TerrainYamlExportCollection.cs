using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Terrains
{
	public sealed class TerrainYamlExportCollection : AssetsExportCollection<ITerrainData>
	{
		public TerrainYamlExportCollection(IAssetExporter assetExporter, ITerrainData terrainData) : base(assetExporter, terrainData)
		{
			AddAssets(terrainData.GetSplatAlphaTextures());
		}
	}
}
