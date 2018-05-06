using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;

namespace UtinyRipper.AssetExporters
{
	public class TextureExportCollection : AssetExportCollection
	{
		public TextureExportCollection(IAssetExporter assetExporter, Object textureAsset) :
			this(assetExporter, (Texture2D)textureAsset)
		{
		}

		public TextureExportCollection(IAssetExporter assetExporter, Texture2D texture):
			base(assetExporter, texture, CreateImporter(texture))
		{
		}
		
		protected static IAssetImporter CreateImporter(Texture2D texture)
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
