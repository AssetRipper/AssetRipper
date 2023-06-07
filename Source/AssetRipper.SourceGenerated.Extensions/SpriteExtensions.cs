using AssetRipper.Assets.Metadata;
using AssetRipper.Numerics;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.SpriteAtlasData;
using System.Drawing;
using System.Numerics;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class SpriteExtensions
	{
		/// <summary>
		/// Pure
		/// </summary>
		/// <param name="sprite"></param>
		/// <param name="atlas"></param>
		/// <param name="sAtlasRect"></param>
		/// <param name="sAtlasPivot"></param>
		/// <param name="sAtlasBorder"></param>
		public static void GetSpriteCoordinatesInAtlas(this ISprite sprite, ISpriteAtlas? atlas, out RectangleF sAtlasRect, out Vector2 sAtlasPivot, out Vector4 sAtlasBorder)
		{
			// sprite values are relative to original image (image, it was created from).
			// since atlas shuffle and crop sprite images, we need to recalculate those values.
			// if sprite doesn't belong to an atlas, consider its image as single sprite atlas

			Vector2 cropBotLeft;
			if (atlas is null || !sprite.Has_RenderDataKey_C213())
			{
				sAtlasRect = sprite.RD_C213.TextureRect.CastToStruct();
				cropBotLeft = (Vector2)sprite.RD_C213.TextureRectOffset;
			}
			else
			{
				ISpriteAtlasData atlasData = atlas.RenderDataMap_C687078895[sprite.RenderDataKey_C213];
				sAtlasRect = atlasData.TextureRect.CastToStruct();
				cropBotLeft = (Vector2)atlasData.TextureRectOffset;
			}

			Vector2 sizeDelta = sprite.Rect_C213.Size() - sAtlasRect.Size();
			Vector2 cropTopRight = new Vector2(sizeDelta.X - cropBotLeft.X, sizeDelta.Y - cropBotLeft.Y);

			Vector2 pivot;
			if (sprite.Has_Pivot_C213())
			{
				pivot = (Vector2)sprite.Pivot_C213;
			}
			else
			{
				Vector2 center = new Vector2(sprite.Rect_C213.Size().X / 2.0f, sprite.Rect_C213.Size().Y / 2.0f);
				Vector2 pivotOffset = center + (Vector2)sprite.Offset_C213;
				pivot = new Vector2(pivotOffset.X / sprite.Rect_C213.Size().X, pivotOffset.Y / sprite.Rect_C213.Size().Y);
			}

			Vector2 pivotPosition = new Vector2(pivot.X * sprite.Rect_C213.Size().X, pivot.Y * sprite.Rect_C213.Size().Y);
			Vector2 aAtlasPivotPosition = pivotPosition - cropBotLeft;
			sAtlasPivot = new Vector2(aAtlasPivotPosition.X / sAtlasRect.Size().X, aAtlasPivotPosition.Y / sAtlasRect.Size().Y);

			if (sprite.Has_Border_C213())
			{
				float borderL = sprite.Border_C213.X == 0.0f ? 0.0f : sprite.Border_C213.X - cropBotLeft.X;
				float borderB = sprite.Border_C213.Y == 0.0f ? 0.0f : sprite.Border_C213.Y - cropBotLeft.Y;
				float borderR = sprite.Border_C213.Z == 0.0f ? 0.0f : sprite.Border_C213.Z - cropTopRight.X;
				float borderT = sprite.Border_C213.W == 0.0f ? 0.0f : sprite.Border_C213.W - cropTopRight.Y;
				sAtlasBorder = new Vector4(borderL, borderB, borderR, borderT);
			}
			else
			{
				sAtlasBorder = default;
			}
		}

		public static ITexture2D? TryGetTexture(this ISprite sprite)
		{
			return sprite.RD_C213.Texture.TryGetAsset(sprite.Collection);
		}
	}
}
