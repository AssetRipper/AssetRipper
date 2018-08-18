using System;
using System.Collections.Generic;
using UtinyRipper.Classes;
using UtinyRipper.Classes.Textures;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.AssetExporters.Classes
{
	public sealed class TextureImporter : DefaultImporter
	{
		public TextureImporter(Texture2D texture)
		{
			if (texture == null)
			{
				throw new ArgumentNullException(nameof(texture));
			}
			m_texture = texture;
		}

		private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 4;
		}
		
		protected override void ExportYAMLPreInner(IExportContainer container, YAMLMappingNode node)
		{
			base.ExportYAMLPreInner(container, node);

			YAMLMappingNode fileNode = YAMLMappingNode.Empty;
			if(m_sprites.Count > 0)
			{
				fileNode = new YAMLMappingNode();
				foreach(Sprite sprite in m_sprites)
				{
					long exportID = container.GetExportID(sprite);
					fileNode.Add(exportID.ToString(), sprite.Name);
				}
			}
			node.Add("fileIDToRecycleName", fileNode);
		}

		protected override void ExportYAMLInner(IExportContainer container, YAMLMappingNode node)
		{
			base.ExportYAMLInner(container, node);
			
			SpriteMetaData[] sprites = new SpriteMetaData[m_sprites.Count];
			for (int i = 0; i < m_sprites.Count; i++)
			{
				sprites[i] = new SpriteMetaData(m_sprites[i]);
			}

			node.AddSerializedVersion(GetSerializedVersion(container.Version));

			YAMLMappingNode mipmap = new YAMLMappingNode();
			mipmap.Add("mipMapMode", (int)TextureImporterMipFilter.BoxFilter);
			mipmap.Add("enableMipMap", m_texture.MipCount > 1 ? true : false);
			mipmap.Add("sRGBTexture", m_texture.ColorSpace == ColorSpace.Gamma ? true : false);
			mipmap.Add("linearTexture", false);
			mipmap.Add("fadeOut", false);
			mipmap.Add("borderMipMap", false);
			mipmap.Add("mipMapsPreserveCoverage", false);
			mipmap.Add("alphaTestReferenceValue", 0.5f);
			mipmap.Add("mipMapFadeDistanceStart", 1);
			mipmap.Add("mipMapFadeDistanceEnd", 3);
			node.Add("mipmaps", mipmap);

			YAMLMappingNode bumpmap = new YAMLMappingNode();
			bumpmap.Add("convertToNormalMap", false);
			bumpmap.Add("externalNormalMap", false);
			bumpmap.Add("heightScale", 0.25f);
			bumpmap.Add("normalMapFilter", (int)TextureImporterNormalFilter.Standard);
			node.Add("bumpmap", bumpmap);

			node.Add("isReadable", m_texture.IsReadable);
			node.Add("grayScaleToAlpha", false);
			node.Add("generateCubemap", (int)TextureImporterGenerateCubemap.AutoCubemap);
			node.Add("cubemapConvolution", 0);
			node.Add("seamlessCubemap", false);
			node.Add("textureFormat", (int)m_texture.TextureFormat);

			int maxSize = m_texture.Width > m_texture.Height ? m_texture.Width : m_texture.Height;
			maxSize = maxSize > 2048 ? maxSize : 2048;
			node.Add("maxTextureSize", maxSize);

			TextureImportSettings importSettings = new TextureImportSettings(m_texture.TextureSettings);
			node.Add("textureSettings", importSettings.ExportYAML(container));

			node.Add("nPOTScale", false);
			node.Add("lightmap", false);
			node.Add("compressionQuality", 50);

			SpriteImportMode spriteMode;
			switch(m_sprites.Count)
			{
				case 0:
					spriteMode = SpriteImportMode.Single;
					break;

				case 1:
					Sprite sprite = m_sprites[0];
					if(sprite.Rect == sprite.RD.TextureRect)
					{
						spriteMode = sprite.Name == m_texture.Name ? SpriteImportMode.Single : SpriteImportMode.Multiple;
					}
					else
					{
						spriteMode = SpriteImportMode.Multiple;
					}
					break;

				default:
					spriteMode = SpriteImportMode.Multiple;
					break;
			}
			node.Add("spriteMode", (int)spriteMode);

			node.Add("spriteExtrude", 1);
			node.Add("spriteMeshType", (int)SpriteMeshType.Tight);
			node.Add("alignment", 0);

			Vector2f pivot = new Vector2f(0.5f, 0.5f);
			node.Add("spritePivot", pivot.ExportYAML(container));
			
			node.Add("spriteBorder", default(Rectf).ExportYAML(container));
			node.Add("spritePixelsToUnits", 100);
			node.Add("alphaUsage", true);
			node.Add("alphaIsTransparency", true);
			node.Add("spriteTessellationDetail", -1);

			TextureImporterType type;
			if (m_texture.LightmapFormat.IsNormalmap())
			{
				type = TextureImporterType.NormalMap;
			}
			else
			{
				type = m_sprites.Count == 0 ? TextureImporterType.Default : TextureImporterType.Sprite;
			}
			node.Add("textureType", (int)type);

			TextureImporterShape shape = (m_texture is Cubemap) ? TextureImporterShape.TextureCube : TextureImporterShape.Texture2D;
			node.Add("textureShape", (int)shape);

			node.Add("maxTextureSizeSet", false);
			node.Add("compressionQualitySet", false);
			node.Add("textureFormatSet", false);

			TextureImporterPlatformSettings platform = new TextureImporterPlatformSettings();
			TextureImporterPlatformSettings[] platforms = new TextureImporterPlatformSettings[] { platform };
			node.Add("platformSettings", platforms.ExportYAML(container));

			SpriteSheetMetaData spriteSheet;
			if(spriteMode == SpriteImportMode.Single)
			{
				if(sprites.Length == 0)
				{
					spriteSheet = new SpriteSheetMetaData(sprites);
				}
				else
				{
					spriteSheet = new SpriteSheetMetaData(sprites[0]);
				}
			}
			else
			{
				spriteSheet = new SpriteSheetMetaData(sprites);
			}
			node.Add("spriteSheet", spriteSheet.ExportYAML(container));

			node.Add("spritePackingTag", string.Empty);
		}

		public override string Name => nameof(TextureImporter);
		
		public IReadOnlyList<Sprite> Sprites
		{
			set => m_sprites = value ?? throw new ArgumentNullException();
		}

		private Texture2D m_texture;
		private IReadOnlyList<Sprite> m_sprites;
	}
}
