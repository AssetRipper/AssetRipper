using AssetRipper.Classes;
using AssetRipper.Classes.TextureImporters;

namespace AssetRipper.Converters
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
