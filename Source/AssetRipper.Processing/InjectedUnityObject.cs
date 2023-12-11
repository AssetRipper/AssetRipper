using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using System.Runtime.CompilerServices;

namespace AssetRipper.Processing;

public abstract class InjectedUnityObject : UnityObjectBase
{
	protected InjectedUnityObject(AssetInfo assetInfo) : base(assetInfo)
	{
	}

	public override void WalkEditor(AssetWalker walker) => WalkStandard(walker);

	public override void WalkRelease(AssetWalker walker) => WalkStandard(walker);

	protected static void WalkPrimitiveField<TSelf, TPrimitive>(AssetWalker walker, TPrimitive value, [CallerArgumentExpression(nameof(value))] string name = "")
		where TSelf : IUnityAssetBase
	{
		if (walker.EnterField<TSelf>(name))
		{
			walker.VisitPrimitive(value);
			walker.ExitField<TSelf>(name);
		}
	}

	protected static void WalkPPtrField<TSelf, TPPtr, TAsset>(AssetWalker walker, TPPtr value, [CallerArgumentExpression(nameof(value))] string name = "")
		where TSelf : IUnityAssetBase
		where TPPtr : IPPtr<TAsset>
		where TAsset : IUnityObjectBase
	{
		if (walker.EnterField<TSelf>(name))
		{
			walker.VisitPPtr<TPPtr, TAsset>(value);
			walker.ExitField<TSelf>(name);
		}
	}

	protected static void WalkPPtrField<TSelf, TAsset>(AssetWalker walker, PPtr<TAsset> value, [CallerArgumentExpression(nameof(value))] string name = "")
		where TSelf : IUnityAssetBase
		where TAsset : IUnityObjectBase
	{
		if (walker.EnterField<TSelf>(name))
		{
			walker.VisitPPtr(value);
			walker.ExitField<TSelf>(name);
		}
	}

	protected void WalkPPtrField<TSelf, TAsset>(AssetWalker walker, TAsset asset, [CallerArgumentExpression(nameof(asset))] string name = "")
		where TSelf : IUnityAssetBase
		where TAsset : IUnityObjectBase
	{
		WalkPPtrField<TSelf, TAsset>(walker, Collection.ForceCreatePPtr(asset), name);
	}
}
