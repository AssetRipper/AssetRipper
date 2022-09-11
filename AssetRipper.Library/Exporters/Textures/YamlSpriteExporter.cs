using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Subclasses.SecondarySpriteTexture;
using AssetRipper.SourceGenerated.Subclasses.SpriteAtlasData;
using AssetRipper.SourceGenerated.Subclasses.SpriteRenderData;

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
			if (sprite.Has_SpriteAtlas_C213() && sprite.Has_RenderDataKey_C213() && sprite.SpriteAtlas_C213.TryGetAsset(sprite.SerializedFile) is { } atlas)
			{
				ISpriteAtlasData spriteData = atlas.RenderDataMap_C687078895[sprite.RenderDataKey_C213];
				ISpriteRenderData m_RD = sprite.RD_C213;
				m_RD.Texture.CopyValues(ConvertPPtr(spriteData.Texture, atlas, sprite));
				if (m_RD.Has_AlphaTexture())
				{
					m_RD.AlphaTexture.CopyValues(ConvertPPtr(spriteData.AlphaTexture, atlas, sprite));
				}
				m_RD.TextureRect.CopyValues(spriteData.TextureRect);
				m_RD.TextureRectOffset.CopyValues(spriteData.TextureRectOffset);
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
				
				// Must clear the reference to SpriteAtlas, since Unity Editor will crash trying to pack an already-packed sprite otherwise.
				sprite.SpriteAtlas_C213.SetValues(0, 0);
				sprite.AtlasTags_C213?.Clear();
			}

			// The m_Rect field is the rect of the sprite in the original texture, which should never be used.
			// Prior to Unity 5, this value in the exported project is used to sample the sprite in the packed atlas.
			// Thus we must fill it with the actual rect in the atlas.
			sprite.Rect_C213.CopyValues(sprite.RD_C213.TextureRect);
			sprite.Offset_C213.CopyValues(sprite.RD_C213.TextureRectOffset);

			return new AssetExportCollection(this, asset);
		}
		
		private static IPPtr<T> ConvertPPtr<T>(IPPtr<T> pptr, ISpriteAtlas atlas, ISprite sprite) where T: IUnityObjectBase
		{
			T? asset = pptr.TryGetAsset(atlas.SerializedFile);
			return asset is not null ? sprite.SerializedFile.CreatePPtr(asset) : new PPtr<T>();
		}
	}
}
