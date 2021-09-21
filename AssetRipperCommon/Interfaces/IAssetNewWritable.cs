using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Interfaces
{
	public interface IAssetNewWritable
	{
		void WriteRelease(AssetWriter writer);
		void WriteDebug(AssetWriter writer);
	}
}
