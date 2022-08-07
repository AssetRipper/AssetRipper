using AssetRipper.Assets.Bundles;
using AssetRipper.VersionUtilities;

namespace AssetRipper.Assets.Collections;

/// <summary>
/// A collection of assets read from a SerializedFile.
/// </summary>
public class SerializedAssetCollection : AssetCollection
{
	public string Name { get; private set; } = string.Empty;
	public UnityVersion Version { get; private set; }

	protected SerializedAssetCollection(Bundle bundle) : base(bundle)
	{
	}

	protected override bool IsValidDependency(AssetCollection dependency)
	{
		return dependency is SerializedAssetCollection or ProcessedAssetCollection;
	}
}

