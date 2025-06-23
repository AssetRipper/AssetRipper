using AssetRipper.SourceGenerated.Classes.ClassID_212;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SpriteRendererExtensions
{
	public static SpriteTileMode GetTileMode(this ISpriteRenderer renderer)
	{
		return (SpriteTileMode)renderer.SpriteTileMode;
	}

	public static void SetTileMode(this ISpriteRenderer renderer, SpriteTileMode tileMode)
	{
		renderer.SpriteTileMode = (int)tileMode;
	}
}
