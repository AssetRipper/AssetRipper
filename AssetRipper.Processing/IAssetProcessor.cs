using AssetRipper.Assets.Bundles;

namespace AssetRipper.Processing
{
	public interface IAssetProcessor
	{
		void Process(GameBundle gameBundle, UnityVersion projectVersion);
	}
}
