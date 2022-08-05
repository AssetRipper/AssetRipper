using AssetRipper.Assets.Collections;

namespace AssetRipper.Assets.Bundles;

/// <summary>
/// A <see cref="Bundle"/> for non-bundled <see cref="AssetCollection"/>s.
/// </summary>
public class LooseFilesBundle : Bundle
{
	public override string Name => nameof(LooseFilesBundle);
}
