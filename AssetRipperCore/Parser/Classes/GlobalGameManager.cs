using AssetRipper.Parser.Asset;

namespace AssetRipper.Parser.Classes
{
	public abstract class GlobalGameManager : GameManager
	{
		protected GlobalGameManager(AssetInfo assetInfo) : base(assetInfo) { }

		public override string ExportPath => ClassID.ToString();
	}
}
