using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;

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
