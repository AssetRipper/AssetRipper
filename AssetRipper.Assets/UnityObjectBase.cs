using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.Assets;

/// <summary>
/// The artificial base class for all generated Unity classes which inherit from Object.
/// </summary>
public abstract class UnityObjectBase : UnityAssetBase, IUnityObjectBase
{
	protected UnityObjectBase(AssetInfo assetInfo)
	{
		AssetInfo = assetInfo;
	}

	public AssetInfo AssetInfo { get; }
	public AssetCollection Collection => AssetInfo.Collection;
	public int ClassID => AssetInfo.ClassID;
	public long PathID => AssetInfo.PathID;
	public virtual string ClassName => GetType().Name;
}
