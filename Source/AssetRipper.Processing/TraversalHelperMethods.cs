using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using System.Runtime.CompilerServices;

namespace AssetRipper.Processing;

public static class TraversalHelperMethods
{
	public static void WalkPrimitiveField<TPrimitive>(this IUnityAssetBase @this, AssetWalker walker, TPrimitive value, [CallerArgumentExpression(nameof(value))] string name = "")
	{
		if (walker.EnterField(@this, name))
		{
			walker.VisitPrimitive(value);
			walker.ExitField(@this, name);
		}
	}

	public static void WalkPrimitiveListField<TPrimitive>(this IUnityAssetBase @this, AssetWalker walker, AssetList<TPrimitive> list, [CallerArgumentExpression(nameof(list))] string name = "")
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

	public static void WalkPPtrField<TAsset>(this IUnityAssetBase @this, AssetWalker walker, IPPtr<TAsset> value, [CallerArgumentExpression(nameof(value))] string name = "")
		where TAsset : IUnityObjectBase
	{
		if (walker.EnterField(@this, name))
		{
			walker.VisitPPtr<TAsset>(value);
			walker.ExitField(@this, name);
		}
	}

	public static void WalkPPtrField<TAsset>(this IUnityAssetBase @this, AssetWalker walker, PPtr<TAsset> value, [CallerArgumentExpression(nameof(value))] string name = "")
		where TAsset : IUnityObjectBase
	{
		if (walker.EnterField(@this, name))
		{
			walker.VisitPPtr(value);
			walker.ExitField(@this, name);
		}
	}

	public static void WalkPPtrField<TAsset>(this IUnityObjectBase @this, AssetWalker walker, TAsset? asset, [CallerArgumentExpression(nameof(asset))] string name = "")
		where TAsset : IUnityObjectBase
	{
		if (walker.EnterField(@this, name))
		{
			@this.VisitPPtr(walker, asset);
			walker.ExitField(@this, name);
		}
	}

	public static void VisitPPtr<TAsset>(this IUnityObjectBase @this, AssetWalker walker, TAsset? asset)
		where TAsset : IUnityObjectBase
	{
		walker.VisitPPtr(@this.Collection.ForceCreatePPtr(asset));
	}

	public static void WalkStandardAssetField<TAsset>(this IUnityAssetBase @this, AssetWalker walker, TAsset asset, [CallerArgumentExpression(nameof(asset))] string name = "")
		where TAsset : IUnityAssetBase
	{
		if (walker.EnterField(@this, name))
		{
			asset.WalkStandard(walker);
			walker.ExitField(@this, name);
		}
	}

	public static void WalkStandardAssetListField<TAsset>(this IUnityAssetBase @this, AssetWalker walker, AssetList<TAsset> list, [CallerArgumentExpression(nameof(list))] string name = "")
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
