namespace uTinyRipper.Classes
{
	public abstract class Motion : NamedObject
	{
		protected Motion(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		protected Motion(AssetInfo assetInfo, uint hideFlags) :
			base(assetInfo, hideFlags)
		{
		}
	}
}
