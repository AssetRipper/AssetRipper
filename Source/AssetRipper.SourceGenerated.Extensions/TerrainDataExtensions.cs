using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Texture2D;

namespace AssetRipper.SourceGenerated.Extensions;

public static class TerrainDataExtensions
{
	public static IEnumerable<ITexture2D> GetSplatAlphaTextures(this ITerrainData terrainData)
	{
		return terrainData.SplatDatabase.AlphaTextures.ToPPtrAccessList<IPPtr_Texture2D, ITexture2D>(terrainData.Collection).WhereNotNull();
	}
}
