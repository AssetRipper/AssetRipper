using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Export.Dependencies;
using AssetRipper.Assets.IO;

namespace AssetRipper.Assets;

public interface IUnityAssetBase : IAsset, IDependent
{
	void CopyValues(IUnityAssetBase? source, PPtrConverter converter);
	void Reset();
}
