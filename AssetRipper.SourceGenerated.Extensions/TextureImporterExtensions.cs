using AssetRipper.SourceGenerated.Classes.ClassID_1006;
using AssetRipper.SourceGenerated.Enums;

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
	}
}
