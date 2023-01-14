using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.SecondarySpriteTexture;
using AssetRipper.SourceGenerated.Subclasses.SpriteAtlasData;
using AssetRipper.SourceGenerated.Subclasses.SpriteRenderData;
using System.Drawing;
using System.Numerics;

namespace AssetRipper.Processing.Textures
{
	public sealed class SpriteProcessor : IAssetProcessor
	{
		public void Process(GameBundle gameBundle, UnityVersion projectVersion)
		{
			foreach (IUnityObjectBase asset in gameBundle.FetchAssets())
			{
				if (asset is ISprite sprite)
				{
					ITexture2D? texture = sprite.RD_C213.Texture.TryGetAsset(sprite.Collection);
					if (texture is not null)
					{
						texture.SpriteInformation ??= new();
						ISpriteAtlas? atlas = sprite.SpriteAtlas_C213P;
						AddToDictionary(sprite, atlas, texture.SpriteInformation);
					}

					ProcessSprite(sprite);
				}
				else if (asset is ISpriteAtlas atlas && atlas.RenderDataMap_C687078895.Count > 0)
				{
					foreach (ISprite? packedSprite in atlas.PackedSprites_C687078895P)
					{
						if (packedSprite is not null
							&& atlas.RenderDataMap_C687078895.TryGetValue(packedSprite.RenderDataKey_C213!, out ISpriteAtlasData? atlasData))
						{
							ITexture2D? texture = atlasData.Texture.TryGetAsset(atlas.Collection);
							if (texture is not null)
							{
								texture.SpriteInformation ??= new();
								AddToDictionary(packedSprite, atlas, texture.SpriteInformation);
							}
						}
					}
				}
			}
		}

		private static void AddToDictionary(ISprite sprite, ISpriteAtlas? atlas, Dictionary<ISprite, ISpriteAtlas?> spriteDictionary)
		{
			if (spriteDictionary.TryGetValue(sprite, out ISpriteAtlas? mappedAtlas))
			{
				if (mappedAtlas is null)
				{
					spriteDictionary[sprite] = atlas;
				}
				else if (atlas is not null && atlas != mappedAtlas)
				{
					throw new Exception($"{nameof(atlas)} is not the same as {nameof(mappedAtlas)}");
				}
			}
			else
			{
				spriteDictionary.Add(sprite, atlas);
			}
		}

		private static void ProcessSprite(ISprite sprite)
		{
			// SpriteAtlas is a new feature since Unity 2017.
			// For older versions of Unity, SpriteAtlas doesn't exist,
			// and the correct metadata of Sprite is stored in the m_RD field.
			// Otherwise, if a SpriteAtlas reference is serialized into this sprite,
			// we must recover the m_RD field of the sprite from the SpriteAtlas.
			ISpriteAtlas? atlas = sprite.SpriteAtlas_C213P;
			if (atlas is not null && sprite.Has_SpriteAtlas_C213())
			{
				if (sprite.Has_RenderDataKey_C213() &&
					atlas.RenderDataMap_C687078895.TryGetValue(sprite.RenderDataKey_C213, out ISpriteAtlasData? spriteData))
				{
					PPtrConverter converter = new PPtrConverter(atlas, sprite);
					ISpriteRenderData m_RD = sprite.RD_C213;
					m_RD.Texture.CopyValues(spriteData.Texture, converter);
					if (m_RD.Has_AlphaTexture())
					{
						m_RD.AlphaTexture.CopyValues(spriteData.AlphaTexture, converter);
					}
					m_RD.TextureRect.CopyValues(spriteData.TextureRect);
					if (m_RD.Has_AtlasRectOffset() && spriteData.Has_AtlasRectOffset())
					{
						m_RD.AtlasRectOffset.CopyValues(spriteData.AtlasRectOffset);
					}
					m_RD.SettingsRaw = spriteData.SettingsRaw;
					if (m_RD.Has_UvTransform())
					{
						m_RD.UvTransform.CopyValues(spriteData.UvTransform);
					}
					m_RD.DownscaleMultiplier = spriteData.DownscaleMultiplier;
					if (m_RD.Has_SecondaryTextures() && spriteData.Has_SecondaryTextures())
					{
						m_RD.SecondaryTextures.Clear();
						foreach (SecondarySpriteTexture spt in spriteData.SecondaryTextures)
						{
							SecondarySpriteTexture newSpt = m_RD.SecondaryTextures.AddNew();
							newSpt.Name.CopyValues(spt.Name);
							newSpt.Texture.CopyValues(spt.Texture, converter);
						}
					}
				}

				// Must clear the reference to SpriteAtlas, since Unity Editor will crash trying to pack an already-packed sprite otherwise.
				sprite.SpriteAtlas_C213.SetNull();
				sprite.AtlasTags_C213.Clear();
			}

			// Some sprite properties must be recalculated with regard to SpriteAtlas. See the comments inside the following method.
			sprite.GetSpriteCoordinatesInAtlas(atlas, out RectangleF rect, out Vector2 pivot, out Vector4 border);
			sprite.Rect_C213.CopyValues(rect);
			if (sprite.Has_Pivot_C213())
			{
				sprite.Pivot_C213.CopyValues(pivot);
			}
			if (sprite.Has_Border_C213())
			{
				sprite.Border_C213.CopyValues(border);
			}

			// Calculate and overwrite Offset. It is the offset in pixels of the pivot to the center of Rect.
			sprite.Offset_C213.X = (pivot.X - 0.5f) * rect.Width;
			sprite.Offset_C213.Y = (pivot.Y - 0.5f) * rect.Height;

			// Calculate and overwrite TextureRectOffset. It is the offset in pixels of m_RD.TextureRect to Rect.
			sprite.RD_C213.TextureRectOffset.X = sprite.RD_C213.TextureRect.X - rect.X;
			sprite.RD_C213.TextureRectOffset.Y = sprite.RD_C213.TextureRect.Y - rect.Y;
		}
	}
}
