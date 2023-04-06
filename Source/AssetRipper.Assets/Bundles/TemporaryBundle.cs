using AssetRipper.Assets.Collections;

namespace AssetRipper.Assets.Bundles;

/// <summary>
/// A <see cref="Bundle"/> containing <see cref="TemporaryAssetCollection"/>s.
/// </summary>
public sealed class TemporaryBundle : VirtualBundle<TemporaryAssetCollection>
{
	public override string Name => nameof(TemporaryBundle);

	public TemporaryAssetCollection AddNew() => new TemporaryAssetCollection(this);
}
