using AssetRipper.Assets;
using AssetRipper.Assets.Traversal;
using AssetRipper.Processing;

namespace AssetRipper.Tests.Traversal;

internal sealed class ColorRGB : UnityAssetBase
{
	private readonly float r = 1;
	private readonly float g = 0.5f;
	private readonly float b = 0;

	public override bool FlowMappedInYaml => true;

	public override void WalkStandard(AssetWalker walker)
	{
		if (walker.EnterAsset(this))
		{
			this.WalkPrimitiveField(walker, r);
			walker.DivideAsset(this);
			this.WalkPrimitiveField(walker, g);
			walker.DivideAsset(this);
			this.WalkPrimitiveField(walker, b);
			walker.ExitAsset(this);
		}
	}
}
