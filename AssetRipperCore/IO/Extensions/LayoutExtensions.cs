using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;

namespace AssetRipper.Core.IO.Extensions
{
	public static class LayoutExtensions
	{
		public static AssetLayout Layout(this AssetReader _this)
		{
			return new AssetLayout(_this.Info);
		}
		public static AssetLayout Layout(this AssetWriter _this)
		{
			return new AssetLayout(_this.Info);
		}
	}
}
