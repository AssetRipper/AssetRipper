using AssetRipper.Parser.Asset;

namespace AssetRipper.Classes
{
	public abstract class LevelGameManager : GameManager
	{
		protected LevelGameManager(AssetInfo assetInfo) : base(assetInfo) { }
	}
}