using AssetRipper.Assets.Bundles;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.VersionUtilities;

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

	public void SetLayout(UnityVersion version, BuildTarget platform, TransferInstructionFlags flags)
	{
		Version = version;
		Platform = platform;
		Flags = flags;
	}
}
