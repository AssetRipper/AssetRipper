using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using System.Diagnostics;
using System.Reflection;

namespace AssetRipper.Assets.Debugging;

internal sealed class DebuggingWalker(AssetCollection collection, List<FieldNameValuePair> fields) : AssetWalker
{
	private bool rootAsset = true;
	private readonly Stack<object?> stack = new();

	public static FieldNameValuePair[] GetFields(IUnityAssetBase asset, AssetCollection collection)
	{
		List<FieldNameValuePair> fields = new();
		DebuggingWalker walker = new(collection, fields);
		asset.WalkStandard(walker);
		return fields.ToArray();
	}

	public override bool EnterField(IUnityAssetBase asset, string name)
	{
		stack.Push(name);
		return true;
	}

	public override void ExitField(IUnityAssetBase asset, string name)
	{
		object? fieldValue = stack.Pop();
		object? fieldName = stack.Pop();
		Debug.Assert(fieldName is string, "Expected field name to be a string.");

#pragma warning disable IL2075 // 'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
		string? fieldType = asset.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.FieldType.ToString();
#pragma warning restore IL2075 // 'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.

		fields.Add(new((string)fieldName, fieldValue, fieldType));
	}

	public override bool EnterAsset(IUnityAssetBase asset)
	{
		if (rootAsset)
		{
			// Ignore the root asset
			rootAsset = false;
			return true;
		}

		stack.Push(new UnityAssetBaseWithCollection(asset, collection));
		return false; // Do not visit children of the asset
	}

	public override bool EnterDictionary<TKey, TValue>(IReadOnlyCollection<KeyValuePair<TKey, TValue>> dictionary)
	{
		stack.Push(new List<AssetKeyValuePair>(dictionary.Count));
		return true;
	}

	public override void DivideDictionary<TKey, TValue>(IReadOnlyCollection<KeyValuePair<TKey, TValue>> dictionary)
	{
		AssetKeyValuePair pair = (AssetKeyValuePair)stack.Pop()!;
		List<AssetKeyValuePair> list = (List<AssetKeyValuePair>)stack.Peek()!;
		list.Add(pair);
	}

	public override void ExitDictionary<TKey, TValue>(IReadOnlyCollection<KeyValuePair<TKey, TValue>> dictionary)
	{
		if (dictionary.Count > 0)
		{
			DivideDictionary(dictionary);
		}
		List<AssetKeyValuePair> list = (List<AssetKeyValuePair>)stack.Pop()!;
		stack.Push(list.ToArray());
	}

	public override bool EnterList<T>(IReadOnlyList<T> _list)
	{
		if (typeof(T) == typeof(string) || typeof(T) == typeof(Utf8String) || typeof(T).IsPrimitive)
		{
			stack.Push(_list);
			return false;
		}

		stack.Push(new List<object?>(_list.Count));
		return true;
	}

	public override void DivideList<T>(IReadOnlyList<T> _list)
	{
		object? value = stack.Pop();
		List<object?> list = (List<object?>)stack.Peek()!;
		list.Add(value);
	}

	public override void ExitList<T>(IReadOnlyList<T> _list)
	{
		if (_list.Count > 0)
		{
			DivideList(_list);
		}
		List<object?> list = (List<object?>)stack.Pop()!;
		stack.Push(list.ToArray());
	}

	public override void ExitDictionaryPair<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
	{
		object? value = stack.Pop();
		object? key = stack.Pop();
		stack.Push(new AssetKeyValuePair(key, value));
	}

	public override void ExitPair<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
	{
		object? value = stack.Pop();
		object? key = stack.Pop();
		stack.Push(new KeyValuePair<object?, object?>(key, value));
	}

	public override void VisitPPtr<TAsset>(PPtr<TAsset> pptr)
	{
		TAsset? asset = collection.TryGetAsset(pptr);
		stack.Push(asset);
	}

	public override void VisitPrimitive<T>(T value)
	{
		stack.Push(value);
	}
}
