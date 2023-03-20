using AssetRipper.Assets.Collections;

namespace AssetRipper.Assets.Metadata;

public readonly record struct AssetInfo(AssetCollection Collection, long PathID, int ClassID)
{
}
