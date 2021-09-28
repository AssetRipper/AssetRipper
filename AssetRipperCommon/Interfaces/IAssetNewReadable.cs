using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Interfaces
{
	public interface IAssetNewReadable : IAssetReadable
	{
		void ReadRelease(AssetReader reader);
		void ReadEditor(AssetReader reader);
	}
}
