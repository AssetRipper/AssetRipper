using System;
using System.Collections.Generic;
using System.Linq;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Sprites;
using uTinyRipper.Classes.Textures;
using uTinyRipper.YAML;

namespace uTinyRipper.AssetExporters.Classes
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
			return 4;
		}
		
		protected override void ExportYAMLPreInner(IExportContainer container, YAMLMappingNode node)
		{
			base.ExportYAMLPreInner(container, node);

			YAMLMappingNode fileNode = YAMLMappingNode.Empty;
			if (m_sprites.Count > 0)
			{
				fileNode = new YAMLMappingNode();
				foreach (Sprite sprite in m_sprites.Keys)
				{
					long exportID = container.GetExportID(sprite);
					fileNode.Add(exportID, sprite.Name);
				}
			}
			node.Add("fileIDToRecycleName", fileNode);
		}

		protected override void ExportYAMLInner(IExportContainer container, YAMLMappingNode node)
		{
			base.ExportYAMLInner(container, node);

			int index = 0;
			SpriteMetaData[] sprites = new SpriteMetaData[m_sprites.Count];
			foreach (KeyValuePair<Sprite, SpriteAtlas> kvp in m_sprites)
			{
				sprites[index++] = new SpriteMetaData(kvp.Key, kvp.Value);
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

			node.Add("nPOTScale", (int)TextureImporterNPOTScale.None);
			node.Add("lightmap", false);
			node.Add("compressionQuality", 50);

			SpriteImportMode spriteMode;
			uint extrude;
			SpriteMeshType meshType;
			SpriteAlignment alignment;
			Vector2f pivot;
			Vector4f border;
			float pixelPerUnit;
			switch (m_sprites.Count)
			{
				case 0:
					{
						spriteMode = SpriteImportMode.Single;
						extrude = 1;
						meshType = SpriteMeshType.Tight;
						alignment = SpriteAlignment.Center;
						pivot = new Vector2f(0.5f, 0.5f);
						border = default;
						pixelPerUnit = 100.0f;
					}
					break;

				case 1:
					{
						Sprite sprite = m_sprites.Keys.First();
						if (sprite.Rect == sprite.RD.TextureRect)
						{
							spriteMode = sprite.Name == m_texture.Name ? SpriteImportMode.Single : SpriteImportMode.Multiple;
						}
						else
						{
							spriteMode = SpriteImportMode.Multiple;
						}
						extrude = sprite.Extrude;
						meshType = sprite.RD.MeshType;
						alignment = SpriteAlignment.Custom;
						pivot = sprite.Pivot;
						border = sprite.Border;
						pixelPerUnit = sprite.PixelsToUnits;
					}
					break;

				default:
					{
						Sprite sprite = m_sprites.Keys.First();
						spriteMode = SpriteImportMode.Multiple;
						extrude = sprite.Extrude;
						meshType = sprite.RD.MeshType;
						alignment = SpriteAlignment.Center;
						pivot = new Vector2f(0.5f, 0.5f);
						border = default;
						pixelPerUnit = sprite.PixelsToUnits;
					}
					break;
			}
			node.Add("spriteMode", (int)spriteMode);
			node.Add("spriteExtrude", extrude);
			node.Add("spriteMeshType", (int)meshType);
			node.Add("alignment", (int)alignment);

			node.Add("spritePivot", pivot.ExportYAML(container));			
			node.Add("spriteBorder", border.ExportYAML(container));
			node.Add("spritePixelsToUnits", pixelPerUnit);
			node.Add("alphaUsage", (int)TextureImporterAlphaSource.FromInput);
			node.Add("alphaIsTransparency", true);
			node.Add("spriteTessellationDetail", -1.0f);

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

			TextureImporterPlatformSettings platform = new TextureImporterPlatformSettings(m_texture.TextureFormat.ToDefaultFormat());
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
		
		public IReadOnlyDictionary<Sprite, SpriteAtlas> Sprites
		{
			set => m_sprites = value ?? throw new ArgumentNullException();
		}

		private Texture2D m_texture;
		private IReadOnlyDictionary<Sprite, SpriteAtlas> m_sprites;
	}
}
