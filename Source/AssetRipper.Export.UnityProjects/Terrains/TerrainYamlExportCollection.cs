using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.Export.UnityProjects.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Terrains
{
	public sealed class TerrainYamlExportCollection : AssetsExportCollection
	{
		public TerrainYamlExportCollection(IAssetExporter assetExporter, ITerrainData terrainData) : base(assetExporter, terrainData)
		{
			foreach (ITexture2D alphaTexture in terrainData.GetSplatAlphaTextures())
			{
				AddAsset(alphaTexture);
			}
		}
	}
}
