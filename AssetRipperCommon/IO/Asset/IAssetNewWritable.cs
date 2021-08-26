namespace AssetRipper.Core.IO.Asset
{
	public interface IAssetNewWritable
	{
		void WriteRelease(AssetWriter writer);
		void WriteDebug(AssetWriter writer);
	}
}
