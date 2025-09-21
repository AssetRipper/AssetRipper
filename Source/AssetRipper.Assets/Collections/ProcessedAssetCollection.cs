using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Assets.Collections;

/// <summary>
/// A collection of artificial assets generated during asset processing.
/// </summary>
public sealed class ProcessedAssetCollection : VirtualAssetCollection
{
	private long m_nextId;

	public ProcessedAssetCollection(Bundle bundle) : base(bundle)
	{
	}

	public new string Name
	{
		get => base.Name;
		set => base.Name = value;
	}

	public void SetLayout(UnityVersion version)
	{
		SetLayout(version, BuildTarget.NoTarget, TransferInstructionFlags.NoTransferInstructionFlags);
	}

	public void SetLayout(UnityVersion version, BuildTarget platform, TransferInstructionFlags flags)
	{
		Version = version;
		OriginalVersion = version;
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

	public TAsset CreateAsset<TData, TAsset>(int classID, TData data, Func<AssetInfo, TData, TAsset> factory) where TAsset : IUnityObjectBase
	{
		AssetInfo assetInfo = CreateAssetInfo(classID);
		TAsset asset = factory(assetInfo, data);
		AddAsset(asset);
		return asset;
	}

	private AssetInfo CreateAssetInfo(int classID)
	{
		return new AssetInfo(this, ++m_nextId, classID);
	}
}

