using System;
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

			node.Add("fileIDToRecycleName", YAMLMappingNode.Empty);
		}

		protected override void ExportYAMLInner(IExportContainer container, YAMLMappingNode node)
		{
			base.ExportYAMLInner(container, node);

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
			node.Add("spriteMode", (int)SpriteImportMode.Single);
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

			TextureImporterType type = m_texture.LightmapFormat.IsNormalmap() ? TextureImporterType.NormalMap : TextureImporterType.Default;
			node.Add("textureType", (int)type);

			TextureImporterShape shape = (m_texture is Cubemap) ? TextureImporterShape.TextureCube : TextureImporterShape.Texture2D;
			node.Add("textureShape", (int)shape);

			node.Add("maxTextureSizeSet", false);
			node.Add("compressionQualitySet", false);
			node.Add("textureFormatSet", false);

			TextureImporterPlatformSettings platform = new TextureImporterPlatformSettings();
			TextureImporterPlatformSettings[] platforms = new TextureImporterPlatformSettings[] { platform };
			node.Add("platformSettings", platforms.ExportYAML(container));

			SpriteMetaData[] sprites = new SpriteMetaData[0];
			SpriteSheetMetaData spriteSheet = new SpriteSheetMetaData(sprites);
			node.Add("spriteSheet", spriteSheet.ExportYAML(container));

			node.Add("spritePackingTag", string.Empty);
		}

		public override string Name => nameof(TextureImporter);

		private readonly Texture2D m_texture;
	}
}
