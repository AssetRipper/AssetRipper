using uTinyRipper.Classes;
using uTinyRipper.Classes.TextureImporters;

namespace uTinyRipper.Converters
{
	public static class CubemapConverter
	{
		public static TextureImporter GeenrateTextureImporter(IExportContainer container, Cubemap origin)
		{
			TextureImporter importer = Texture2DConverter.GenerateTextureImporter(container, origin);
			importer.TextureShape = TextureImporterShape.TextureCube;
			return importer;
		}
	}
}
