using AssetRipper.SourceGenerated.Subclasses.Heightmap;

namespace AssetRipper.SourceGenerated.Extensions;

public static class HeightmapExtensions
{
	public static int GetWidth(this IHeightmap heightmap)
	{
		return heightmap.Has_Width() ? heightmap.Width : heightmap.Resolution;
	}

	public static int GetHeight(this IHeightmap heightmap)
	{
		return heightmap.Has_Height() ? heightmap.Height : heightmap.Resolution;
	}
}
