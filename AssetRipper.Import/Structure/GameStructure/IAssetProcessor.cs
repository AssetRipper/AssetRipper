using AssetRipper.Assets.Bundles;

namespace AssetRipper.Core.Structure.GameStructure
{
	public interface IAssetProcessor
	{
		void Process(GameBundle gameBundle, UnityVersion projectVersion);
	}
}
