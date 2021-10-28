using AssetRipper.Core.Parser.Asset;

namespace AssetRipper.Core.Classes
{
	public abstract class LevelGameManager : GameManager, ILevelGameManager
	{
		protected LevelGameManager(AssetInfo assetInfo) : base(assetInfo) { }
	}
}