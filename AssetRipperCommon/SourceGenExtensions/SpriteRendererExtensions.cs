using AssetRipper.Core.Classes.SpriteRenderer;
using AssetRipper.SourceGenerated.Classes.ClassID_212;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class SpriteRendererExtensions
	{
		public static SpriteDrawMode GetDrawMode(this ISpriteRenderer renderer)
		{
			return (SpriteDrawMode)renderer.DrawMode_C212;
		}

		public static void SetDrawMode(this ISpriteRenderer renderer, SpriteDrawMode drawMode)
		{
			renderer.DrawMode_C212 = (int)drawMode;
		}

		public static SpriteTileMode GetTileMode(this ISpriteRenderer renderer)
		{
			return (SpriteTileMode)renderer.SpriteTileMode_C212;
		}

		public static void SetTileMode(this ISpriteRenderer renderer, SpriteTileMode tileMode)
		{
			renderer.SpriteTileMode_C212 = (int)tileMode;
		}

		public static SpriteMaskInteraction GetMaskInteraction(this ISpriteRenderer renderer)
		{
			return (SpriteMaskInteraction)renderer.MaskInteraction_C212;
		}

		public static void SetMaskInteraction(this ISpriteRenderer renderer, SpriteMaskInteraction maskInteraction)
		{
			renderer.MaskInteraction_C212 = (int)maskInteraction;
		}

		public static SpriteSortPoint GetSpriteSortPoint(this ISpriteRenderer renderer)
		{
			return (SpriteSortPoint)renderer.SpriteSortPoint_C212;
		}

		public static void SetSpriteSortPoint(this ISpriteRenderer renderer, SpriteSortPoint spriteSortPoint)
		{
			renderer.SpriteSortPoint_C212 = (int)spriteSortPoint;
		}
	}
}
