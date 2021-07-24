using AssetRipper.Parser.Asset;

namespace AssetRipper.Parser.Classes
{
	public abstract class LevelGameManager : GameManager
	{
		protected LevelGameManager(AssetInfo assetInfo) : base(assetInfo) { }
	}
}