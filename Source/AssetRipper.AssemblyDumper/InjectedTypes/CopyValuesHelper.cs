#nullable disable

using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.AssemblyDumper.InjectedTypes;

internal static class CopyValuesHelper
{
	public static T[] DuplicateArray<T>(T[] array)
	{
		if (array is null || array.Length == 0)
		{
			return Array.Empty<T>();
		}
		else
		{
			T[] copy = new T[array.Length];
			Array.Copy(array, copy, array.Length);
			return copy;
		}
	}
	public static T[][] DuplicateArrayArray<T>(T[][] array)
	{
		if (array is null || array.Length == 0)
		{
			return Array.Empty<T[]>();
		}
		else
		{
			T[][] copy = new T[array.Length][];
			for (int i = 0; i < array.Length; i++)
			{
				copy[i] = DuplicateArray(array[i]);
			}
			return copy;
		}
	}
	public static PPtr ConvertPPtr<T>(IPPtr sourcePPtr, PPtrConverter converter) where T : IUnityObjectBase
	{
		PPtr pptr = new(sourcePPtr.FileID, sourcePPtr.PathID);
		if (converter.SourceCollection != converter.TargetCollection || converter.Resolver.GetType() != typeof(DefaultAssetResolver))
		{
			return converter.Convert<T>(pptr);
		}
		else
		{
			return pptr;
		}
	}
	public static void CopyCapacityFrom_Dictionary<TTargetKey, TTargetValue, TSourceKey, TSourceValue>(AssetDictionary<TTargetKey, TTargetValue> target, AccessDictionaryBase<TSourceKey, TSourceValue> source)
		where TTargetKey : notnull, new()
		where TTargetValue : notnull, new()
		where TSourceKey : notnull
		where TSourceValue : notnull
	{
		if (source is null or { Count: 0 })
		{
			target.Clear();
		}
		else if (target.Count < source.Count)
		{
			for (int i = target.Count; i < source.Count; i++)
			{
				target.AddNew();
			}
		}
		else if (target.Count > source.Count)
		{
			for (int i = target.Count - 1; i >= source.Count; i--)
			{
				target.RemoveAt(i);
			}
		}
		target.Capacity = target.Count;
	}
	public static void CopyCapacityFrom_List<TTarget, TSource>(AssetList<TTarget> target, AccessListBase<TSource> source)
		where TTarget : notnull, new()
		where TSource : notnull
	{
		if (source is null or { Count: 0 })
		{
			target.Clear();
		}
		else if (target.Count < source.Count)
		{
			for (int i = target.Count; i < source.Count; i++)
			{
				target.AddNew();
			}
		}
		else if (target.Count > source.Count)
		{
			for (int i = target.Count - 1; i >= source.Count; i--)
			{
				target.RemoveAt(i);
			}
		}
		target.Capacity = target.Count;
	}
}

#nullable enable
