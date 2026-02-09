using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using System.Runtime.CompilerServices;

#nullable disable

namespace AssetRipper.AssemblyDumper.InjectedTypes;

internal static class EqualityComparisonHelper
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool? GetNull() => null;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool? GetTrue() => true;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool? GetFalse() => false;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNull(this bool? value) => value == null;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNotNull(this bool? value) => value != null;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsTrue(this bool? value) => value == true;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsFalse(this bool? value) => value == false;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool? ToTrilean(this bool value) => value;

	public static bool ByteArrayEquals(byte[] x, byte[] y)
	{
		return x.AsSpan().SequenceEqual(y);
	}

	// eg string, int, bool, Vector3f
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool EquatableEquals<T>(T x, T y) where T : IEquatable<T>
	{
		return x.Equals(y);
	}

	// eg AssetList<float>, AssetList<Utf8String>, or AssetList<Vector3f>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool EquatableListEquals<T>(AssetList<T> x, AssetList<T> y) where T : IEquatable<T>, new()
	{
		return RuntimeHelpers.IsReferenceOrContainsReferences<T>()
			? x.SequenceEqual(y)
			: x.GetSpan().SequenceEqual(y.GetSpan());
	}

	public static bool EquatableListListEquals<T>(AssetList<AssetList<T>> x, AssetList<AssetList<T>> y) where T : IEquatable<T>, new()
	{
		if (x.Count != y.Count)
		{
			return false;
		}

		for (int i = 0; i < x.Count; i++)
		{
			if (!EquatableListEquals(x[i], y[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static bool EquatablePairEquals<TKey, TValue>(AssetPair<TKey, TValue> x, AssetPair<TKey, TValue> y) where TKey : IEquatable<TKey>, new() where TValue : IEquatable<TValue>, new()
	{
		return x.Key.Equals(y.Key) && x.Value.Equals(y.Value);
	}

	public static bool EquatableDictionaryEquals<TKey, TValue>(AssetDictionary<TKey, TValue> x, AssetDictionary<TKey, TValue> y) where TKey : IEquatable<TKey>, new() where TValue : IEquatable<TValue>, new()
	{
		if (x.Count != y.Count)
		{
			return false;
		}

		for (int i = 0; i < x.Count; i++)
		{
			if (!EquatablePairEquals(x.GetPair(i), y.GetPair(i)))
			{
				return false;
			}
		}
		return true;
	}

	public static bool EquatableDictionaryListEquals<TKey, TValueElement>(AssetDictionary<TKey, AssetList<TValueElement>> x, AssetDictionary<TKey, AssetList<TValueElement>> y) where TKey : IEquatable<TKey>, new() where TValueElement : IEquatable<TValueElement>, new()
	{
		if (x.Count != y.Count)
		{
			return false;
		}

		for (int i = 0; i < x.Count; i++)
		{
			if (!x.GetKey(i).Equals(y.GetKey(i)) || !EquatableListEquals(x.GetValue(i), y.GetValue(i)))
			{
				return false;
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool? AssetEquals<T>(T x, T y, AssetEqualityComparer comparer) where T : IUnityAssetBase
	{
		return x.AddToEqualityComparer(y, comparer);
	}

	// eg AssetList<ChildAnimatorState>
	public static bool? AssetListEquals<T>(AssetList<T> x, AssetList<T> y, AssetEqualityComparer comparer) where T : new()
	{
		if (x.Count != y.Count)
		{
			return false;
		}

		bool? result = true;
		for (int i = 0; i < x.Count; i++)
		{
			switch (GenericAssetEquals(x[i], y[i], comparer))
			{
				case false:
					return false;
				case null:
					result = null;
					break;
			}
		}
		return result;
	}

	public static bool? AssetPairEquals<TKey, TValue>(AssetPair<TKey, TValue> x, AssetPair<TKey, TValue> y, AssetEqualityComparer comparer)
		where TKey : new()
		where TValue : new()
	{
		bool? keyResult = GenericAssetEquals(x.Key, y.Key, comparer);
		if (keyResult == false)
		{
			return false;
		}
		bool? valueResult = GenericAssetEquals(x.Value, y.Value, comparer);
		if (valueResult == false)
		{
			return false;
		}

		if (keyResult == null || valueResult == null)
		{
			return null;
		}
		return true;
	}

	public static bool? AssetDictionaryEquals<TKey, TValue>(AssetDictionary<TKey, TValue> x, AssetDictionary<TKey, TValue> y, AssetEqualityComparer comparer)
		where TKey : new()
		where TValue : new()
	{
		if (x.Count != y.Count)
		{
			return false;
		}

		bool? result = true;
		for (int i = 0; i < x.Count; i++)
		{
			switch (AssetPairEquals(x.GetPair(i), y.GetPair(i), comparer))
			{
				case false:
					return false;
				case null:
					result = null;
					break;
			}
		}
		return result;
	}

	public static bool? AssetPairListEquals<TKey, TValue>(AssetList<AssetPair<TKey, TValue>> x, AssetList<AssetPair<TKey, TValue>> y, AssetEqualityComparer comparer)
		where TKey : new()
		where TValue : new()
	{
		if (x.Count != y.Count)
		{
			return false;
		}

		bool? result = true;
		for (int i = 0; i < x.Count; i++)
		{
			switch (AssetPairEquals(x[i], y[i], comparer))
			{
				case false:
					return false;
				case null:
					result = null;
					break;
			}
		}
		return result;
	}

	public static bool? AssetDictionaryListEquals<TKey, TValueElement>(AssetDictionary<TKey, AssetList<TValueElement>> x, AssetDictionary<TKey, AssetList<TValueElement>> y, AssetEqualityComparer comparer)
		where TKey : new()
		where TValueElement : new()
	{
		if (x.Count != y.Count)
		{
			return false;
		}

		bool? result = true;
		for (int i = 0; i < x.Count; i++)
		{
			switch (GenericAssetEquals(x.GetKey(i), y.GetKey(i), comparer))
			{
				case false:
					return false;
				case null:
					result = null;
					break;
			}
			switch (AssetListEquals(x.GetValue(i), y.GetValue(i), comparer))
			{
				case false:
					return false;
				case null:
					result = null;
					break;
			}
		}
		return result;
	}

	public static bool? AssetDictionaryPairEquals<TPairKey, TPairValue, TValue>(AssetDictionary<AssetPair<TPairKey, TPairValue>, TValue> x, AssetDictionary<AssetPair<TPairKey, TPairValue>, TValue> y, AssetEqualityComparer comparer)
		where TPairKey : new()
		where TPairValue : new()
		where TValue : new()
	{
		if (x.Count != y.Count)
		{
			return false;
		}

		bool? result = true;
		for (int i = 0; i < x.Count; i++)
		{
			switch (AssetPairEquals(x.GetKey(i), y.GetKey(i), comparer))
			{
				case false:
					return false;
				case null:
					result = null;
					break;
			}
			switch (GenericAssetEquals(x.GetValue(i), y.GetValue(i), comparer))
			{
				case false:
					return false;
				case null:
					result = null;
					break;
			}
		}
		return result;
	}

	private static bool? GenericAssetEquals<T>(T x, T y, AssetEqualityComparer comparer)
	{
		if (typeof(T).IsAssignableTo(typeof(IUnityAssetBase)))
		{
			return ((IUnityAssetBase)x).AddToEqualityComparer((IUnityAssetBase)y, comparer);
		}
		else
		{
			return EqualityComparer<T>.Default.Equals(x, y);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool? MaybeAddDependentComparison(IPPtr x, IPPtr y, AssetEqualityComparer comparer)
	{
		return comparer.MaybeAddDependentComparison(x, y);
	}

	public static bool? MonoBehaviourStructureEquals(IUnityAssetBase x, IUnityAssetBase y, AssetEqualityComparer comparer)
	{
		return (x, y) switch
		{
			(not null, not null) => x.AddToEqualityComparer(y, comparer),
			(null, null) => true,
			_ => false,
		};
	}
}

#nullable enable