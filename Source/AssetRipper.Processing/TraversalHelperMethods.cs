using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using System.Runtime.CompilerServices;

namespace AssetRipper.Processing;

public static class TraversalHelperMethods
{
	public static void WalkPrimitiveField<TSelf, TPrimitive>(this TSelf @this, AssetWalker walker, TPrimitive value, [CallerArgumentExpression(nameof(value))] string name = "")
		where TSelf : IUnityAssetBase
	{
		if (walker.EnterField(@this, name))
		{
			walker.VisitPrimitive(value);
			walker.ExitField(@this, name);
		}
	}

	public static void WalkPrimitiveListField<TSelf, TPrimitive>(this TSelf @this, AssetWalker walker, AssetList<TPrimitive> list, [CallerArgumentExpression(nameof(list))] string name = "")
		where TSelf : IUnityAssetBase
		where TPrimitive : notnull, new()
	{
		if (walker.EnterField(@this, name))
		{
			if (walker.EnterList(list))
			{
				int count = list.Count;
				if (count > 0)
				{
					int i = 0;
					while (true)
					{
						walker.VisitPrimitive(list[i]);
						i++;
						if (i >= count)
						{
							break;
						}
						walker.DivideList(list);
					}
				}
				walker.ExitList(list);
			}
			walker.ExitField(@this, name);
		}
	}

	public static void WalkPPtrField<TSelf, TPPtr, TAsset>(this TSelf @this, AssetWalker walker, TPPtr value, [CallerArgumentExpression(nameof(value))] string name = "")
		where TSelf : IUnityAssetBase
		where TPPtr : IPPtr<TAsset>
		where TAsset : IUnityObjectBase
	{
		if (walker.EnterField(@this, name))
		{
			walker.VisitPPtr<TPPtr, TAsset>(value);
			walker.ExitField(@this, name);
		}
	}

	public static void WalkPPtrField<TSelf, TAsset>(this TSelf @this, AssetWalker walker, PPtr<TAsset> value, [CallerArgumentExpression(nameof(value))] string name = "")
		where TSelf : IUnityAssetBase
		where TAsset : IUnityObjectBase
	{
		if (walker.EnterField(@this, name))
		{
			walker.VisitPPtr(value);
			walker.ExitField(@this, name);
		}
	}

	public static void WalkPPtrField<TSelf, TAsset>(this TSelf @this, AssetWalker walker, TAsset? asset, [CallerArgumentExpression(nameof(asset))] string name = "")
		where TSelf : IUnityObjectBase
		where TAsset : IUnityObjectBase
	{
		if (walker.EnterField(@this, name))
		{
			@this.VisitPPtr(walker, asset);
			walker.ExitField(@this, name);
		}
	}

	public static void VisitPPtr<TSelf, TAsset>(this TSelf @this, AssetWalker walker, TAsset? asset)
		where TSelf : IUnityObjectBase
		where TAsset : IUnityObjectBase
	{
		walker.VisitPPtr(@this.Collection.ForceCreatePPtr(asset));
	}

	public static void WalkStandardAssetField<TSelf, TAsset>(this TSelf @this, AssetWalker walker, TAsset asset, [CallerArgumentExpression(nameof(asset))] string name = "")
		where TSelf : IUnityAssetBase
		where TAsset : IUnityAssetBase
	{
		if (walker.EnterField(@this, name))
		{
			asset.WalkStandard(walker);
			walker.ExitField(@this, name);
		}
	}

	public static void WalkStandardAssetListField<TSelf, TAsset>(this TSelf @this, AssetWalker walker, AssetList<TAsset> list, [CallerArgumentExpression(nameof(list))] string name = "")
		where TSelf : IUnityAssetBase
		where TAsset : IUnityAssetBase, new()
	{
		if (walker.EnterField(@this, name))
		{
			if (walker.EnterList(list))
			{
				int count = list.Count;
				if (count > 0)
				{
					int i = 0;
					while (true)
					{
						list[i].WalkStandard(walker);
						i++;
						if (i >= count)
						{
							break;
						}
						walker.DivideList(list);
					}
				}
				walker.ExitList(list);
			}
			walker.ExitField(@this, name);
		}
	}
}
