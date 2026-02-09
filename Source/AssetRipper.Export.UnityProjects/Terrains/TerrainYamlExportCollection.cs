using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Terrains;

public sealed class TerrainYamlExportCollection : AssetsExportCollection<ITerrainData>
{
	public TerrainYamlExportCollection(IAssetExporter assetExporter, ITerrainData terrainData) : base(assetExporter, terrainData)
	{
		foreach (ITexture2D texture in terrainData.GetSplatAlphaTextures())
		{
			//Sometimes TerrainData can be duplicated, but retain the same alpha textures.
			//https://github.com/AssetRipper/AssetRipper/issues/1356
			if (texture.MainAsset == terrainData)
			{
				AddAsset(texture);
			}
		}
	}
}
