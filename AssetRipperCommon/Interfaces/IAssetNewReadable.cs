using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Interfaces
{
	public interface IAssetNewReadable
	{
		void ReadRelease(AssetReader reader);
		void ReadDebug(AssetReader reader);
	}
}
