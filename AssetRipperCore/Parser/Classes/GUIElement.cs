using AssetRipper.Parser.Asset;

namespace AssetRipper.Parser.Classes
{
	public abstract class GUIElement : Behaviour
	{
		public GUIElement(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}
	}
}
