using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;

namespace UtinyRipper.AssetExporters
{
	public class TextureExportCollection : AssetExportCollection
	{
		public TextureExportCollection(IAssetExporter assetExporter, Texture2D texture):
			base(assetExporter, texture, CreateImporter(texture))
		{
		}
		
		protected static IYAMLExportable CreateImporter(Texture2D texture)
		{
			if (Config.IsConvertTexturesToPNG)
			{
#warning TODO: texture exporter
				return new NativeFormatImporter(texture);
				throw new System.NotImplementedException();
			}
			else
			{
				return new IHVImageFormatImporter(texture);
			}
		}
	}
}
