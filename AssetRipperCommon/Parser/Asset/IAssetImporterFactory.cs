using AssetRipper.Core.Classes.Meta.Importers;
using AssetRipper.Core.Layout;

namespace AssetRipper.Core.Parser.Asset
{
	public interface IAssetImporterFactory
	{
		INativeFormatImporter CreateNativeFormatImporter(AssetLayout layout);
	}
}
