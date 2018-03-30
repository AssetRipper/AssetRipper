using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;

namespace UtinyRipper.AssetExporters
{
	public class TextureExportCollection : AssetExportCollection
	{
		public TextureExportCollection(IAssetExporter assetExporter, Object texture):
			base(assetExporter, texture)
		{
		}
		
		protected override IYAMLExportable CreateImporter(Object asset)
		{
			Texture2D texture = (Texture2D)asset;
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
