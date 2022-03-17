using AssetRipper.Core.Classes.Meta.Importers;
using AssetRipper.Core.Layout;

namespace AssetRipper.Core.Parser.Asset
{
	public class LegacyImporterFactory : IAssetImporterFactory
	{
		public IDefaultImporter CreateDefaultImporter(LayoutInfo layout)
		{
			return new DefaultImporter(layout);
		}

		public IMonoImporter CreateMonoImporter(LayoutInfo layout)
		{
			return new MonoImporter(layout);
		}

		public INativeFormatImporter CreateNativeFormatImporter(LayoutInfo layout)
		{
			return new NativeFormatImporter(layout);
		}
	}
}
