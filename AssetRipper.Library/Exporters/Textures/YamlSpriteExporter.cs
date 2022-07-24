using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Library.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;

namespace AssetRipper.Library.Exporters.Textures
{
	/// <summary>
	/// Exports sprites as YAML assets.
	/// </summary>
	public sealed class YamlSpriteExporter : YamlExporterBase
	{
		private SpriteExportMode SpriteExportMode { get; }

		public YamlSpriteExporter(LibraryConfiguration configuration)
		{
			SpriteExportMode = configuration.SpriteExportMode;
		}

		public override bool IsHandle(IUnityObjectBase asset) =>
			SpriteExportMode == SpriteExportMode.Yaml && asset is ISprite or ISpriteAtlas;

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			if (asset is ISpriteAtlas)
			{
				return new EmptyExportCollection();
			}
			
			// If a SpriteAtlas reference is serialized into this sprite,
			// we must recover the m_RD field of the sprite from the SpriteAtlas.
			ISprite sprite = asset as ISprite;
			if (sprite.SpriteAtlas_C213.FindAsset(sprite.SerializedFile) is ISpriteAtlas atlas)
			{
				var spriteData = atlas.RenderDataMap_C687078895[sprite.RenderDataKey_C213];
				var m_RD = sprite.RD_C213;
				m_RD.Texture.CopyValues(spriteData.Texture);
				m_RD.AlphaTexture.CopyValues(spriteData.AlphaTexture);
				m_RD.TextureRect.CopyValues(spriteData.TextureRect);
				m_RD.TextureRectOffset.CopyValues(spriteData.TextureRectOffset);
				m_RD.AtlasRectOffset.CopyValues(spriteData.AtlasRectOffset);
				m_RD.SettingsRaw = spriteData.SettingsRaw;
				m_RD.UvTransform.CopyValues(spriteData.UvTransform);
				m_RD.DownscaleMultiplier = spriteData.DownscaleMultiplier;
				m_RD.SecondaryTextures.Clear();
				foreach (var spt in spriteData.SecondaryTextures)
				{
					m_RD.SecondaryTextures.Add(spt);
				}
			}
			sprite.SpriteAtlas_C213.SetValues(0, 0);
			sprite.AtlasTags_C213?.Clear();

			return new AssetExportCollection(this, asset);
		}
	}
}
