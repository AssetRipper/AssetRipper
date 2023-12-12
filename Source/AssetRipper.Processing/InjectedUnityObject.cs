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
}
public abstract class InjectedUnityObject<TSelf> : InjectedUnityObject where TSelf : InjectedUnityObject<TSelf>
{
	protected InjectedUnityObject(AssetInfo assetInfo) : base(assetInfo)
	{
	}

	protected void WalkPrimitiveField<TPrimitive>(AssetWalker walker, TPrimitive value, [CallerArgumentExpression(nameof(value))] string name = "")
	{
		if (walker.EnterField((TSelf)this, name))
		{
			walker.VisitPrimitive(value);
			walker.ExitField((TSelf)this, name);
		}
	}

	protected void WalkPPtrField<TPPtr, TAsset>(AssetWalker walker, TPPtr value, [CallerArgumentExpression(nameof(value))] string name = "")
		where TPPtr : IPPtr<TAsset>
		where TAsset : IUnityObjectBase
	{
		if (walker.EnterField((TSelf)this, name))
		{
			walker.VisitPPtr<TPPtr, TAsset>(value);
			walker.ExitField((TSelf)this, name);
		}
	}

	protected void WalkPPtrField<TAsset>(AssetWalker walker, PPtr<TAsset> value, [CallerArgumentExpression(nameof(value))] string name = "")
		where TAsset : IUnityObjectBase
	{
		if (walker.EnterField((TSelf)this, name))
		{
			walker.VisitPPtr(value);
			walker.ExitField((TSelf)this, name);
		}
	}

	protected void WalkPPtrField<TAsset>(AssetWalker walker, TAsset? asset, [CallerArgumentExpression(nameof(asset))] string name = "")
		where TAsset : IUnityObjectBase
	{
		WalkPPtrField(walker, Collection.ForceCreatePPtr(asset), name);
	}
}
