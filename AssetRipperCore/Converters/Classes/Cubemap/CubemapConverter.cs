using AssetRipper.Converters.Classes.Texture2D;
using AssetRipper.Converters.Project;
using AssetRipper.Parser.Classes.Meta.Importers.Texture;

namespace AssetRipper.Converters.Classes.Cubemap
{
	public static class CubemapConverter
	{
		public static TextureImporter GeenrateTextureImporter(IExportContainer container, Parser.Classes.Cubemap origin)
		{
			TextureImporter importer = Texture2DConverter.GenerateTextureImporter(container, origin);
			importer.TextureShape = TextureImporterShape.TextureCube;
			return importer;
		}
	}
}
