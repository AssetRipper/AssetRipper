using AssetRipper.Parser.Asset;

namespace AssetRipper.Parser.Classes
{
	public abstract class AudioBehaviour : Behaviour
	{
		protected AudioBehaviour(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}
	}
}
