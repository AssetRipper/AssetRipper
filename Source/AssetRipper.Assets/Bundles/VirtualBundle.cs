using AssetRipper.Assets.Collections;

namespace AssetRipper.Assets.Bundles;

/// <summary>
/// Abstract class for virtual bundles.
/// </summary>
/// <typeparam name="T">The Type of the <see cref="VirtualAssetCollection"/>s that this bundle can contain.</typeparam>
public abstract class VirtualBundle<T> : Bundle where T : VirtualAssetCollection
{
	protected sealed override bool IsCompatibleBundle(Bundle bundle)
	{
		return bundle is VirtualBundle<T>;
	}

	protected sealed override bool IsCompatibleCollection(AssetCollection collection)
	{
		return collection is T;
	}
}
