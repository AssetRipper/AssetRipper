namespace AssetRipper.Core.IO.Asset
{
	public interface IAssetNewWritable : IAssetWritable
	{
		void WriteRelease(AssetWriter writer);
		void WriteEditor(AssetWriter writer);
	}
}
