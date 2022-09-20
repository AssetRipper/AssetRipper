using AssetRipper.Assets.Collections;

namespace AssetRipper.Assets.Bundles;

public abstract class VirtualBundle<T> : Bundle where T : VirtualAssetCollection
{
	protected sealed override bool IsCompatibleBundle(Bundle bundle)
	{
		return bundle is T;
	}

	protected sealed override bool IsCompatibleCollection(AssetCollection collection)
	{
		return collection is T;
	}
}
