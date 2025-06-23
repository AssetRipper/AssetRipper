using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.IO.Files;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.SecondarySpriteTexture;
using AssetRipper.SourceGenerated.Subclasses.SpriteAtlasData;
using AssetRipper.SourceGenerated.Subclasses.SpriteRenderData;
using System.Drawing;
using System.Numerics;

namespace AssetRipper.Processing.Textures;

public sealed partial class SpriteProcessor : IAssetProcessor
{
	public void Process(GameData gameData)
	{
		ObjectFactory factory = new ObjectFactory(gameData);
		foreach (IUnityObjectBase asset in gameData.GameBundle
			.FetchAssetCollections()
			.Where(c => !SpecialFileNames.IsDefaultResourceOrBuiltinExtra(c.Name))
			.SelectMany(c => c))
		{
			if (asset is ITexture2D texture)
			{
				if (texture.MainAsset is null)
				{
					factory.GetOrCreate(texture);
				}
			}
			else if (asset is ISprite sprite)
			{
				ITexture2D? spriteTexture = sprite.TryGetTexture();
				if (spriteTexture is not null)
				{
					SpriteInformationObject spriteInformationObject = factory.GetOrCreate(spriteTexture);
					ISpriteAtlas? atlas = sprite.SpriteAtlasP;
					spriteInformationObject.AddToDictionary(sprite, atlas);
				}

				ProcessSprite(sprite);
			}
			else if (asset is ISpriteAtlas atlas && atlas.RenderDataMap.Count > 0)
			{
				foreach (ISprite packedSprite in atlas.PackedSpritesP.WhereNotNull())
				{
					if (TryGetPackedSpriteTexture(atlas, packedSprite, out ITexture2D? spriteTexture))
					{
						SpriteInformationObject spriteInformationObject = factory.GetOrCreate(spriteTexture);
						spriteInformationObject.AddToDictionary(packedSprite, atlas);
					}
				}
			}
		}
		foreach (SpriteInformationObject asset in factory.Assets)
		{
			asset.SetMainAsset();
		}
	}

	private static bool TryGetPackedSpriteTexture(ISpriteAtlas atlas, ISprite packedSprite, [NotNullWhen(true)] out ITexture2D? spriteTexture)
	{
		if (packedSprite.Has_RenderDataKey() && atlas.RenderDataMap.TryGetValue(packedSprite.RenderDataKey, out ISpriteAtlasData? atlasData))
		{
			spriteTexture = atlasData.Texture.TryGetAsset(atlas.Collection);
		}
		else
		{
			spriteTexture = null;
		}
		return spriteTexture is not null;
	}

	private static void ProcessSprite(ISprite sprite)
	{
		// SpriteAtlas is a new feature since Unity 2017.
		// For older versions of Unity, SpriteAtlas doesn't exist,
		// and the correct metadata of Sprite is stored in the m_RD field.
		// Otherwise, if a SpriteAtlas reference is serialized into this sprite,
		// we must recover the m_RD field of the sprite from the SpriteAtlas.
		ISpriteAtlas? atlas = sprite.SpriteAtlasP;
		if (atlas is not null)
		{
			if (sprite.Has_RenderDataKey() &&
				atlas.RenderDataMap.TryGetValue(sprite.RenderDataKey, out ISpriteAtlasData? spriteData))
			{
				PPtrConverter converter = new PPtrConverter(atlas, sprite);
				ISpriteRenderData m_RD = sprite.RD;
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
						m_RD.SecondaryTextures.AddNew().CopyValues(spt, converter);
					}
				}
			}

			// Must clear the reference to SpriteAtlas, since Unity Editor will crash trying to pack an already-packed sprite otherwise.
			sprite.SpriteAtlasP = null;
			sprite.AtlasTags?.Clear();
		}

		// Some sprite properties must be recalculated with regard to SpriteAtlas. See the comments inside the following method.
		sprite.GetSpriteCoordinatesInAtlas(atlas, out RectangleF rect, out Vector2 pivot, out Vector4 border);
		sprite.Rect.CopyValues(rect);
		sprite.Pivot?.CopyValues(pivot);
		sprite.Border?.CopyValues(border);

		// Calculate and overwrite Offset. It is the offset in pixels of the pivot to the center of Rect.
		sprite.Offset.X = (pivot.X - 0.5f) * rect.Width;
		sprite.Offset.Y = (pivot.Y - 0.5f) * rect.Height;

		// Calculate and overwrite TextureRectOffset. It is the offset in pixels of m_RD.TextureRect to Rect.
		sprite.RD.TextureRectOffset.X = sprite.RD.TextureRect.X - rect.X;
		sprite.RD.TextureRectOffset.Y = sprite.RD.TextureRect.Y - rect.Y;
	}
}
