using uTinyRipper.Layout;

namespace uTinyRipper.Classes
{
	public abstract class Motion : NamedObject
	{
		protected Motion(AssetLayout layout) :
			base(layout)
		{
		}

		protected Motion(AssetInfo assetInfo):
			base(assetInfo)
		{
		}
	}
}
