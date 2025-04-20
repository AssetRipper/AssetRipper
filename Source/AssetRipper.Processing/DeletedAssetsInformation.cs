using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;

namespace AssetRipper.Processing;

public sealed class DeletedAssetsInformation : AssetGroup
{
	public DeletedAssetsInformation(AssetInfo assetInfo) : base(assetInfo)
	{
	}

	public List<IUnityObjectBase> DeletedAssets { get; } = new();

	public override List<IUnityObjectBase> Assets => DeletedAssets;

	public override void WalkStandard(AssetWalker walker)
	{
		if (walker.EnterAsset(this))
		{
			this.WalkPPtrListField(walker, DeletedAssets);
			walker.ExitAsset(this);
		}
	}

	public override IEnumerable<(string, PPtr)> FetchDependencies()
	{
		foreach (IUnityObjectBase asset in DeletedAssets)
		{
			yield return ($"{nameof(DeletedAssets)}[]", AssetToPPtr(asset));
		}
	}
}
