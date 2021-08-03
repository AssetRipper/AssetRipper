using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;

namespace AssetRipper.Core.Classes
{
	public abstract class Motion : NamedObject
	{
		protected Motion(AssetLayout layout) : base(layout) { }

		protected Motion(AssetInfo assetInfo) : base(assetInfo) { }
	}
}
