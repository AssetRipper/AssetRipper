using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using System.Diagnostics;

namespace AssetRipper.Processing;

public abstract class AssetGroup : UnityObjectBase
{
	protected AssetGroup(AssetInfo assetInfo) : base(assetInfo)
	{
	}

	public abstract IEnumerable<IUnityObjectBase> Assets { get; }

	public virtual void SetMainAsset()
	{
		MainAsset = this;
		foreach (IUnityObjectBase asset in Assets)
		{
			Debug.Assert(asset.MainAsset is null || asset.MainAsset == this, "Asset already has a main asset assigned.");
			asset.MainAsset = this;
		}
	}

	protected PPtr AssetToPPtr(IUnityObjectBase? asset) => Collection.ForceCreatePPtr(asset);
}
