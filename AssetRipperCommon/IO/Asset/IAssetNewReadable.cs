namespace AssetRipper.Core.IO.Asset
{
	public interface IAssetNewReadable
	{
		void ReadRelease(AssetReader reader);
		void ReadDebug(AssetReader reader);
	}
}
