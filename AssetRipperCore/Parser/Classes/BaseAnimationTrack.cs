using AssetRipper.Parser.Asset;

namespace AssetRipper.Parser.Classes
{
	public abstract class BaseAnimationTrack : NamedObject
	{
		protected BaseAnimationTrack(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}
	}
}