using AssetRipper.Assets.Bundles;

namespace AssetRipper.Assets.Collections;

/// <summary>
/// A collection of artificial assets generated during asset processing.
/// </summary>
public class ProcessedAssetCollection : VirtualAssetCollection
{
	public ProcessedAssetCollection(Bundle bundle) : base(bundle)
	{
	}

	/// <summary>
	/// Subject to change
	/// </summary>
	public const int ProcessedFileIndex = -1;
}

