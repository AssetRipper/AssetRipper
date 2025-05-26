using AssetRipper.Assets;
using AssetRipper.Assets.Traversal;
using AssetRipper.Processing;

namespace AssetRipper.Tests.Traversal;

internal sealed class VectorXY : UnityAssetBase
{
#pragma warning disable CS0649
	private readonly ulong xy;
#pragma warning restore CS0649

	public override int SerializedVersion => 2;

	public override void WalkStandard(AssetWalker walker)
	{
		if (walker.EnterAsset(this))
		{
			this.WalkPrimitiveField(walker, xy);
			walker.ExitAsset(this);
		}
	}
}
