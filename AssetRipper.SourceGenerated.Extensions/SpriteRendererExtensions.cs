using AssetRipper.SourceGenerated.Classes.ClassID_212;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class SpriteRendererExtensions
	{
		public static SpriteTileMode GetTileMode(this ISpriteRenderer renderer)
		{
			return (SpriteTileMode)renderer.SpriteTileMode_C212;
		}

		public static void SetTileMode(this ISpriteRenderer renderer, SpriteTileMode tileMode)
		{
			renderer.SpriteTileMode_C212 = (int)tileMode;
		}
	}
}
