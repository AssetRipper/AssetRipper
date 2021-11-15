using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;

namespace AssetRipper.Core.Classes
{
	public abstract class Motion : NamedObject
	{
		protected Motion(LayoutInfo layout) : base(layout) { }

		protected Motion(AssetInfo assetInfo) : base(assetInfo) { }
	}
}
