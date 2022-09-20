using AssetRipper.Assets.Collections;

namespace AssetRipper.Assets.Bundles;

/// <summary>
/// A <see cref="Bundle"/> containing <see cref="TemporaryAssetCollection"/>s.
/// </summary>
public class TemporaryBundle : VirtualBundle<TemporaryAssetCollection>
{
	public override string Name => nameof(TemporaryBundle);
}
