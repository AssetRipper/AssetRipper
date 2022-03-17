using AssetRipper.Core.Classes.Meta.Importers;
using AssetRipper.Core.Layout;

namespace AssetRipper.Core.Parser.Asset
{
	public interface IAssetImporterFactory
	{
		IDefaultImporter CreateDefaultImporter(LayoutInfo layout);
		IMonoImporter CreateMonoImporter(LayoutInfo layout);
		INativeFormatImporter CreateNativeFormatImporter(LayoutInfo layout);
	}
}
