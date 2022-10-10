using AssetRipper.Assets.Bundles;

namespace AssetRipper.Assets.Collections;

/// <summary>
/// A collection of temporary assets generated during each project export.
/// These get destroyed after export completion.
/// </summary>
public class TemporaryAssetCollection : VirtualAssetCollection
{
	public TemporaryAssetCollection(Bundle bundle) : base(bundle)
	{
	}
}
