using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;

namespace AssetRipper.Processing;

public sealed class DeletedAssetsInformation : UnityObjectBase
{
	public DeletedAssetsInformation(AssetInfo assetInfo) : base(assetInfo)
	{
	}

	public List<IUnityObjectBase> DeletedAssets { get; } = new();

	public override void WalkStandard(AssetWalker walker)
	{
		if (walker.EnterAsset(this))
		{
			//todo: implement
			//This was not implemented because AssetWalker.EnterList(AssetList<T>)
			//should be merged with AssetWalker.EnterArray(T[])
			//to form AssetWalker.EnterList(IReadOnlyList<T>)
			//which would handle both of those and other types of lists.
			walker.ExitAsset(this);
		}
	}

	public override IEnumerable<(string, PPtr)> FetchDependencies()
	{
		foreach (IUnityObjectBase asset in DeletedAssets)
		{
			yield return ($"{nameof(DeletedAssets)}[]", Collection.ForceCreatePPtr(asset));
		}
	}

	public void SetMainAsset()
	{
		MainAsset = this;
		foreach (IUnityObjectBase asset in DeletedAssets)
		{
			asset.MainAsset = this;
		}
	}
}
