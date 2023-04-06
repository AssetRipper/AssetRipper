using AssetRipper.Assets.Bundles;

namespace AssetRipper.Assets.Collections;

/// <summary>
/// A collection of artificial assets generated during asset processing.
/// </summary>
public sealed class ProcessedAssetCollection : VirtualAssetCollection
{
	public ProcessedAssetCollection(Bundle bundle) : base(bundle)
	{
	}

	protected override bool IsCompatibleDependency(AssetCollection dependency)
	{
		return dependency is SerializedAssetCollection or ProcessedAssetCollection;
	}
}

