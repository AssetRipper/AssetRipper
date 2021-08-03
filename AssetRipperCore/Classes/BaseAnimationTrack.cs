using AssetRipper.Core.Parser.Asset;

namespace AssetRipper.Core.Classes
{
	public abstract class BaseAnimationTrack : NamedObject
	{
		protected BaseAnimationTrack(AssetInfo assetInfo) : base(assetInfo) { }
	}
}