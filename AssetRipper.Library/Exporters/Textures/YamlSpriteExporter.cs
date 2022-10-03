using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Subclasses.Rectf;
using AssetRipper.SourceGenerated.Subclasses.SecondarySpriteTexture;
using AssetRipper.SourceGenerated.Subclasses.SpriteAtlasData;
using AssetRipper.SourceGenerated.Subclasses.SpriteRenderData;
using AssetRipper.SourceGenerated.Subclasses.Vector2f;
using AssetRipper.SourceGenerated.Subclasses.Vector4f;

namespace AssetRipper.Library.Exporters.Textures
{
	/// <summary>
	/// Exports sprites as YAML assets.
	/// </summary>
	public sealed class YamlSpriteExporter : YamlExporterBase
	{
		public override bool IsHandle(IUnityObjectBase asset) => asset is ISprite or ISpriteAtlas;

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			if (asset is ISpriteAtlas)
			{
				return new EmptyExportCollection();
			}
			
			// SpriteAtlas is a new feature since Unity 2017.
			// For older versions of Unity, SpriteAtlas doesn't exist,
			// and the correct metadata of Sprite is stored in the m_RD field.
			// Otherwise, if a SpriteAtlas reference is serialized into this sprite,
			// we must recover the m_RD field of the sprite from the SpriteAtlas.
			ISprite sprite = (ISprite)asset;
			ISpriteAtlas? atlas = null;
			if (sprite.Has_SpriteAtlas_C213() && (atlas = sprite.SpriteAtlas_C213.TryGetAsset(sprite.SerializedFile)) != null)
			{
				if (sprite.Has_RenderDataKey_C213() &&
				    atlas.RenderDataMap_C687078895.TryGetValue(sprite.RenderDataKey_C213, out ISpriteAtlasData? spriteData))
				{
					ISpriteRenderData m_RD = sprite.RD_C213;
					m_RD.Texture.CopyValues(ConvertPPtr(spriteData.Texture, atlas, sprite));
					if (m_RD.Has_AlphaTexture())
					{
						m_RD.AlphaTexture.CopyValues(ConvertPPtr(spriteData.AlphaTexture, atlas, sprite));
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
							newSpt.Texture.CopyValues(ConvertPPtr(spt.Texture, atlas, sprite));
						}
					}
				}

				// Must clear the reference to SpriteAtlas, since Unity Editor will crash trying to pack an already-packed sprite otherwise.
				sprite.SpriteAtlas_C213.SetValues(0, 0);
				sprite.AtlasTags_C213?.Clear();
			}

			// Some sprite properties must be recalculated with regard to SpriteAtlas. See the comments inside the following method.
			sprite.GetSpriteCoordinatesInAtlas(atlas, out Rectf rect, out Vector2f_3_5_0_f5 pivot, out Vector4f_3_5_0_f5 border);
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

			return new AssetExportCollection(this, asset);
		}
		
		private static IPPtr<T> ConvertPPtr<T>(IPPtr<T> pptr, ISpriteAtlas atlas, ISprite sprite) where T: IUnityObjectBase
		{
			T? asset = pptr.TryGetAsset(atlas.SerializedFile);
			return asset is not null ? sprite.SerializedFile.CreatePPtr(asset) : new PPtr<T>();
		}
	}
}
