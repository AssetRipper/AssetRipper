using AssetRipper.Converters.Texture2D;
using AssetRipper.Project;
using AssetRipper.Classes.Meta.Importers.Texture;

namespace AssetRipper.Converters.Cubemap
{
	public static class CubemapConverter
	{
		public static TextureImporter GeenrateTextureImporter(IExportContainer container, AssetRipper.Classes.Cubemap origin)
		{
			TextureImporter importer = Texture2DConverter.GenerateTextureImporter(container, origin);
			importer.TextureShape = TextureImporterShape.TextureCube;
			return importer;
		}
	}
}
