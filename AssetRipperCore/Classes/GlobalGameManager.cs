using AssetRipper.Core.Parser.Asset;

namespace AssetRipper.Core.Classes
{
	public abstract class GlobalGameManager : GameManager, IGlobalGameManager
	{
		protected GlobalGameManager(AssetInfo assetInfo) : base(assetInfo) { }

		public override string ExportPath => ClassID.ToString();
	}
}
