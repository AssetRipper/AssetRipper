using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using System.Runtime.CompilerServices;

namespace AssetRipper.Processing;

public static class TraversalHelperMethods
{
	private interface IWalkHandler<TValue>
	{
		static abstract void Walk(AssetWalker walker, TValue value);
	}

	private readonly struct DictionaryWalkHandler<TKey, TValue, TKeyHandler, TValueHandler> : IWalkHandler<IReadOnlyCollection<KeyValuePair<TKey, TValue>>>
		where TKey : notnull
		where TValue : notnull
		where TKeyHandler : IWalkHandler<TKey>
		where TValueHandler : IWalkHandler<TValue>
	{
		public static void Walk(AssetWalker walker, IReadOnlyCollection<KeyValuePair<TKey, TValue>> list)
		{
			if (walker.EnterDictionary(list))
			{
				int count = list.Count;
				if (count > 0)
				{
					int i = 0;
					foreach (KeyValuePair<TKey, TValue> pair in list)
					{
						if (walker.EnterDictionaryPair(pair))
						{
							TKeyHandler.Walk(walker, pair.Key);
							walker.DivideDictionaryPair(pair);
							TValueHandler.Walk(walker, pair.Value);
							walker.ExitDictionaryPair(pair);
						}
						i++;
						if (i >= count)
						{
							break;
						}
						walker.DivideDictionary(list);
					}
				}
				walker.ExitDictionary(list);
			}
		}
	}

	private readonly struct ListWalkHandler<TElement, TElementHandler> : IWalkHandler<IReadOnlyList<TElement>>
		where TElement : notnull
		where TElementHandler : IWalkHandler<TElement>
	{
		public static void Walk(AssetWalker walker, IReadOnlyList<TElement> list)
		{
			if (walker.EnterList(list))
			{
				int count = list.Count;
				if (count > 0)
				{
					int i = 0;
					while (true)
					{
						TElementHandler.Walk(walker, list[i]);
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
		}
	}

	private readonly struct PrimitiveWalkHandler<TPrimitive> : IWalkHandler<TPrimitive>
		where TPrimitive : notnull
	{
		public static void Walk(AssetWalker walker, TPrimitive value)
		{
			walker.VisitPrimitive(value);
		}
	}

	private readonly struct IPPtrWalkHandler<TAsset> : IWalkHandler<IPPtr<TAsset>>
		where TAsset : IUnityObjectBase
	{
		public static void Walk(AssetWalker walker, IPPtr<TAsset> value)
		{
			walker.VisitPPtr(value);
		}
	}

	private readonly struct PPtrWalkHandler<TAsset> : IWalkHandler<PPtr<TAsset>>
		where TAsset : IUnityObjectBase
	{
		public static void Walk(AssetWalker walker, PPtr<TAsset> value)
		{
			walker.VisitPPtr(value);
		}
	}

	private readonly struct AssetStandardWalkHandler<TAsset> : IWalkHandler<TAsset>
		where TAsset : IUnityAssetBase
	{
		public static void Walk(AssetWalker walker, TAsset asset)
		{
			asset.WalkStandard(walker);
		}
	}

	public static void WalkPrimitiveField<TPrimitive>(this IUnityAssetBase @this, AssetWalker walker, TPrimitive value, [CallerArgumentExpression(nameof(value))] string name = "")
		where TPrimitive : notnull
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
			ListWalkHandler<TPrimitive, PrimitiveWalkHandler<TPrimitive>>.Walk(walker, list);
			walker.ExitField(@this, name);
		}
	}

	public static void WalkPPtrField<TAsset>(this IUnityAssetBase @this, AssetWalker walker, IPPtr<TAsset> value, [CallerArgumentExpression(nameof(value))] string name = "")
		where TAsset : IUnityObjectBase
	{
		if (walker.EnterField(@this, name))
		{
			walker.VisitPPtr(value);
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

	public static void WalkPPtrListField<TAsset>(this IUnityAssetBase @this, AssetWalker walker, IReadOnlyList<IPPtr<TAsset>> list, [CallerArgumentExpression(nameof(list))] string name = "")
		where TAsset : IUnityObjectBase
	{
		if (walker.EnterField(@this, name))
		{
			ListWalkHandler<IPPtr<TAsset>, IPPtrWalkHandler<TAsset>>.Walk(walker, list);
			walker.ExitField(@this, name);
		}
	}

	public static void WalkPPtrListField<TAsset>(this IUnityAssetBase @this, AssetWalker walker, IReadOnlyList<PPtr<TAsset>> list, [CallerArgumentExpression(nameof(list))] string name = "")
		where TAsset : IUnityObjectBase
	{
		if (walker.EnterField(@this, name))
		{
			ListWalkHandler<PPtr<TAsset>, PPtrWalkHandler<TAsset>>.Walk(walker, list);
			walker.ExitField(@this, name);
		}
	}

	public static void WalkPPtrListField<TAsset>(this IUnityObjectBase @this, AssetWalker walker, IReadOnlyList<TAsset> list, [CallerArgumentExpression(nameof(list))] string name = "")
		where TAsset : IUnityObjectBase
	{
		if (walker.EnterField(@this, name))
		{
			PPtr<TAsset>[] pptrs = new PPtr<TAsset>[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				pptrs[i] = @this.Collection.ForceCreatePPtr(list[i]);
			}
			ListWalkHandler<PPtr<TAsset>, PPtrWalkHandler<TAsset>>.Walk(walker, pptrs);
			walker.ExitField(@this, name);
		}
	}

	public static void WalkDictionaryPPtrField<TKey, TValue>(this IUnityAssetBase @this, AssetWalker walker, IReadOnlyCollection<KeyValuePair<PPtr<TKey>, PPtr<TValue>>> list, [CallerArgumentExpression(nameof(list))] string name = "")
		where TKey : IUnityObjectBase
		where TValue : IUnityObjectBase
	{
		if (walker.EnterField(@this, name))
		{
			DictionaryWalkHandler<PPtr<TKey>, PPtr<TValue>, PPtrWalkHandler<TKey>, PPtrWalkHandler<TValue>>.Walk(walker, list);
			walker.ExitField(@this, name);
		}
	}

#nullable disable
	public static void WalkDictionaryPPtrField<TKey, TValue>(this IUnityObjectBase @this, AssetWalker walker, IReadOnlyCollection<KeyValuePair<TKey, TValue>> list, [CallerArgumentExpression(nameof(list))] string name = "")
		where TKey : IUnityObjectBase
		where TValue : IUnityObjectBase
	{
		IReadOnlyCollection<KeyValuePair<PPtr<TKey>, PPtr<TValue>>> pairs = [..list.Select(pairs =>
		{
			return new KeyValuePair<PPtr<TKey>, PPtr<TValue>>(@this.Collection.ForceCreatePPtr(pairs.Key), @this.Collection.ForceCreatePPtr(pairs.Value));
		})];

		@this.WalkDictionaryPPtrField(walker, pairs, name);
	}
#nullable restore

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
			ListWalkHandler<TAsset, AssetStandardWalkHandler<TAsset>>.Walk(walker, list);
			walker.ExitField(@this, name);
		}
	}
}
