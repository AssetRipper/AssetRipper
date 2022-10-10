using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.VersionUtilities;

namespace AssetRipper.Assets.Collections;

/// <summary>
/// A collection of artificial assets.
/// </summary>
public abstract class VirtualAssetCollection : AssetCollection
{
	protected VirtualAssetCollection(Bundle bundle) : base(bundle)
	{
	}

	public void SetLayout(UnityVersion version, BuildTarget platform, TransferInstructionFlags flags)
	{
		Version = version;
		Platform = platform;
		Flags = flags;
	}

	public T CreateAsset<T>(int classID, Func<AssetInfo, T> factory) where T : IUnityObjectBase
	{
		AssetInfo assetInfo = CreateAssetInfo(classID);
		T asset = factory(assetInfo);
		AddAsset(asset);
		return asset;
	}

	public T CreateAsset<T>(int classID, Func<UnityVersion, AssetInfo, T> factory) where T : IUnityObjectBase
	{
		AssetInfo assetInfo = CreateAssetInfo(classID);
		T asset = factory(Version, assetInfo);
		AddAsset(asset);
		return asset;
	}

	private AssetInfo CreateAssetInfo(int classID)
	{
		return new AssetInfo(this, ++m_nextId, classID);
	}

	private long m_nextId;
}
