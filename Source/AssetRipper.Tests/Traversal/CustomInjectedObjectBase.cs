using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;

namespace AssetRipper.Tests.Traversal;

internal abstract partial class CustomInjectedObjectBase : UnityObjectBase
{
	protected CustomInjectedObjectBase(AssetInfo assetInfo) : base(assetInfo)
	{
	}

	public override void WalkEditor(AssetWalker walker) => Walk(walker, WalkType.Editor);

	public override void WalkRelease(AssetWalker walker) => Walk(walker, WalkType.Release);

	public override void WalkStandard(AssetWalker walker) => Walk(walker, WalkType.Standard);
}
