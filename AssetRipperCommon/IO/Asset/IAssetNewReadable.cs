namespace AssetRipper.Core.IO.Asset
{
	public interface IAssetNewReadable : IAssetReadable
	{
		void ReadRelease(AssetReader reader);
		void ReadEditor(AssetReader reader);
	}
}
