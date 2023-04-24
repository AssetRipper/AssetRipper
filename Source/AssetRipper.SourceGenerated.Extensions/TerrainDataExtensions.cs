using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_28;

namespace AssetRipper.SourceGenerated.Extensions;

public static class TerrainDataExtensions
{
	public static IEnumerable<ITexture2D> GetSplatAlphaTextures(this ITerrainData terrainData)
	{
		return terrainData.SplatDatabase_C156.AlphaTextures.Select(ptr => ptr.TryGetAsset(terrainData.Collection)).WhereNotNull();
	}
}
