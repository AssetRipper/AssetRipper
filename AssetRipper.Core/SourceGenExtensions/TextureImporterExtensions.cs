using AssetRipper.Core.Classes.Meta.Importers.Texture;
using AssetRipper.Core.Classes.Sprite;
using AssetRipper.SourceGenerated.Classes.ClassID_1006;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class TextureImporterExtensions
	{
		public static TextureImporterMipFilter GetTextureCompression(this ITextureImporter importer)
		{
			return (TextureImporterMipFilter)importer.MipMaps_C1006.MipMapMode;
		}

		public static TextureImporterNormalFilter GetNormalMapFilter(this ITextureImporter importer)
		{
			return (TextureImporterNormalFilter)importer.BumpMap_C1006.NormalMapFilter;
		}

		public static TextureImporterGenerateCubemap GetGenerateCubemap(this ITextureImporter importer)
		{
			return (TextureImporterGenerateCubemap)importer.GenerateCubemap_C1006;
		}

		public static SourceGenerated.Enums.TextureImporterFormat GetTextureFormat(this ITextureImporter importer)
		{
			return importer.Format_C1006E;
		}

		public static TextureImporterNPOTScale GetNPOTScale(this ITextureImporter importer)
		{
			return (TextureImporterNPOTScale)importer.NPOTScale_C1006;
		}

		public static SpriteImportMode GetSpriteMode(this ITextureImporter importer)
		{
			return (SpriteImportMode)importer.SpriteMode_C1006;
		}

		public static SpriteMeshType GetSpriteMeshType(this ITextureImporter importer)
		{
			return (SpriteMeshType)importer.SpriteMeshType_C1006;
		}

		public static SpriteAlignment GetAlignment(this ITextureImporter importer)
		{
			return (SpriteAlignment)importer.Alignment_C1006;
		}

		public static TextureImporterAlphaSource GetAlphaUsage(this ITextureImporter importer)
		{
			return (TextureImporterAlphaSource)importer.AlphaUsage_C1006;
		}

		public static TextureImporterType GetTextureType(this ITextureImporter importer)
		{
			return (TextureImporterType)importer.TextureType_C1006;
		}

		public static TextureImporterShape GetTextureShape(this ITextureImporter importer)
		{
			return (TextureImporterShape)importer.TextureShape_C1006;
		}
	}
}
