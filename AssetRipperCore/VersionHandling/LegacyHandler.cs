using AssetRipper.Core.Parser.Asset;

namespace AssetRipper.Core.VersionHandling
{
	public class LegacyHandler : UnityHandlerBase
	{
		public LegacyHandler()
		{
			this.AssetFactory = new AssetFactory();
			this.ImporterFactory = new LegacyImporterFactory();
			ClassIDTypeEnum = typeof(ClassIDType);
		}
	}
}
