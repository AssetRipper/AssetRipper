using AssetRipper.Assets.Bundles;

namespace AssetRipper.Assets.Collections;

/// <summary>
/// A collection of artificial assets.
/// </summary>
public abstract class VirtualAssetCollection : AssetCollection
{
	protected VirtualAssetCollection(Bundle bundle) : base(bundle)
	{
	}
}
