using AssetRipper.Core.Classes.Meta.Importers;
using AssetRipper.Core.Layout;

namespace AssetRipper.Core.Parser.Asset
{
	public class LegacyImporterFactory : IAssetImporterFactory
	{
		public INativeFormatImporter CreateNativeFormatImporter(AssetLayout layout)
		{
			return new NativeFormatImporter(layout);
		}
	}
}
