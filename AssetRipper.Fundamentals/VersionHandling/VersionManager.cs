using AssetRipper.Core.Parser.Asset;

namespace AssetRipper.Core.VersionHandling
{
	public static class VersionManager
	{
		private static AssetFactoryBase? _assetFactory;
		public static AssetFactoryBase AssetFactory
		{
			get
			{
				return _assetFactory ?? throw new Exception("AssetFactory not set");
			}
			set
			{
				_assetFactory = value;
			}
		}
	}
}
