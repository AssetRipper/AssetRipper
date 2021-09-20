using AssetRipper.Core.Classes.Meta.Importers.Texture;
using AssetRipper.Core.Converters.Texture2D;
using AssetRipper.Core.Project;

namespace AssetRipper.Core.Converters.Cubemap
{
	public static class CubemapConverter
	{
		public static TextureImporter GeenrateTextureImporter(IExportContainer container, AssetRipper.Core.Classes.Cubemap origin)
		{
			TextureImporter importer = Texture2DConverter.GenerateTextureImporter(container, origin);
			importer.TextureShape = TextureImporterShape.TextureCube;
			return importer;
		}
	}
}
