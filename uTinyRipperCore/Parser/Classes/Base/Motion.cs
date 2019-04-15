using uTinyRipper.Classes.Objects;

namespace uTinyRipper.Classes
{
	public abstract class Motion : NamedObject
	{
		protected Motion(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		protected Motion(AssetInfo assetInfo, HideFlags hideFlags) :
			base(assetInfo, hideFlags)
		{
		}
	}
}
