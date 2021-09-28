using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Interfaces
{
	public interface IAssetNewWritable : IAssetWritable
	{
		void WriteRelease(AssetWriter writer);
		void WriteEditor(AssetWriter writer);
	}
}
