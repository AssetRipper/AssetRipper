using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.Processing;
using System.Reflection;

namespace AssetRipper.Tests.Traversal;

partial class CustomInjectedObjectBase
{
	private void Walk(AssetWalker walker, WalkType walkType)
	{
		if (walker.EnterAsset(this))
		{
			FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (fields.Length > 0)
			{
				int i = 0;
				while (true)
				{
					WalkField(walker, fields[i], walkType);
					i++;
					if (i >= fields.Length)
					{
						break;
					}
					walker.DivideAsset(this);
				}
			}
			walker.ExitAsset(this);
		}
	}

	private static MethodInfo GetFirstMethod(string name)
	{
		return typeof(AssetWalker).GetMethods(BindingFlags.Public | BindingFlags.Instance).First(m => m.Name == name);
	}

	private bool EnterList(AssetWalker walker, Type elementType, object? list)
	{
		return (bool)GetFirstMethod(nameof(AssetWalker.EnterList))
			.MakeGenericMethod(elementType)
			.Invoke(walker, [list])!;
	}

	private void ExitList(AssetWalker walker, Type elementType, object? list)
	{
		GetFirstMethod(nameof(AssetWalker.ExitList))
			.MakeGenericMethod(elementType)
			.Invoke(walker, [list]);
	}

	private bool EnterDictionary(AssetWalker walker, Type keyType, Type valueType, object? dictionary)
	{
		return (bool)GetFirstMethod(nameof(AssetWalker.EnterDictionary))
			.MakeGenericMethod(keyType, valueType)
			.Invoke(walker, [dictionary])!;
	}

	private void ExitDictionary(AssetWalker walker, Type keyType, Type valueType, object? dictionary)
	{
		GetFirstMethod(nameof(AssetWalker.ExitDictionary))
			.MakeGenericMethod(keyType, valueType)
			.Invoke(walker, [dictionary]);
	}

	private bool EnterDictionaryPair(AssetWalker walker, Type keyType, Type valueType, object? pair)
	{
		return (bool)GetFirstMethod(nameof(AssetWalker.EnterDictionaryPair))
			.MakeGenericMethod(keyType, valueType)
			.Invoke(walker, [pair])!;
	}

	private void DivideDictionaryPair(AssetWalker walker, Type keyType, Type valueType, object? pair)
	{
		GetFirstMethod(nameof(AssetWalker.DivideDictionaryPair))
			.MakeGenericMethod(keyType, valueType)
			.Invoke(walker, [pair]);
	}

	private void ExitDictionaryPair(AssetWalker walker, Type keyType, Type valueType, object? pair)
	{
		GetFirstMethod(nameof(AssetWalker.ExitDictionaryPair))
			.MakeGenericMethod(keyType, valueType)
			.Invoke(walker, [pair]);
	}

	private bool EnterPair(AssetWalker walker, Type keyType, Type valueType, object? pair)
	{
		return (bool)GetFirstMethod(nameof(AssetWalker.EnterPair))
			.MakeGenericMethod(keyType, valueType)
			.Invoke(walker, [pair])!;
	}

	private void DividePair(AssetWalker walker, Type keyType, Type valueType, object? pair)
	{
		GetFirstMethod(nameof(AssetWalker.DividePair))
			.MakeGenericMethod(keyType, valueType)
			.Invoke(walker, [pair]);
	}

	private void ExitPair(AssetWalker walker, Type keyType, Type valueType, object? pair)
	{
		GetFirstMethod(nameof(AssetWalker.ExitPair))
			.MakeGenericMethod(keyType, valueType)
			.Invoke(walker, [pair]);
	}

	private void WalkField(AssetWalker walker, FieldInfo field, WalkType walkType)
	{
		string name = GetName(field, walkType);
		if (walker.EnterField(this, name))
		{
			VisitValue(walker, walkType, field.FieldType, field.GetValue(this));

			walker.ExitField(this, name);
		}
	}

	private void VisitValue(AssetWalker walker, WalkType walkType, Type type, object? value)
	{
		if (PrimitiveHelper.IsPrimitive(type))
		{
			PrimitiveHelper.VisitPrimitive(walker, type, value);
		}
		else if (type.IsAssignableTo(typeof(IUnityObjectBase)))
		{
			typeof(TraversalHelperMethods).GetMethod(nameof(TraversalHelperMethods.VisitPPtr))!
				.MakeGenericMethod(type)
				.Invoke(null, [this, walker, value]);
		}
		else if (type.IsAssignableTo(typeof(IPPtr)))
		{
			throw new NotImplementedException(type.Name);
		}
		else if (type.IsGenericType)
		{
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			if (genericTypeDefinition == typeof(AssetList<>))
			{
				Type elementType = type.GetGenericArguments()[0];

				object? list = value;
				if (EnterList(walker, elementType, list))
				{
					int count = (int)(type.GetProperty(nameof(AssetList<int>.Count))?.GetValue(list) ?? throw new NullReferenceException());
					if (count > 0)
					{
						MethodInfo divideListMethod = GetFirstMethod(nameof(AssetWalker.DivideList)).MakeGenericMethod(elementType);

						MethodInfo indexer = type.GetMethod("get_Item", BindingFlags.Public | BindingFlags.Instance)!;

						int i = 0;
						while (true)
						{
							VisitValue(walker, walkType, elementType, indexer.Invoke(list, [i]));
							i++;
							if (i >= count)
							{
								break;
							}
							divideListMethod.Invoke(walker, [list]);
						}
					}
					ExitList(walker, elementType, list);
				}
			}
			else if (genericTypeDefinition == typeof(AssetDictionary<,>))
			{
				Type keyType = type.GetGenericArguments()[0];
				Type valueType = type.GetGenericArguments()[1];

				if (EnterDictionary(walker, keyType, valueType, value))
				{
					int count = (int)type.GetProperty(nameof(AssetDictionary<int, int>.Count))?.GetValue(value)!;
					if (count > 0)
					{
						MethodInfo divideDictionaryMethod = GetFirstMethod(nameof(AssetWalker.DivideDictionary)).MakeGenericMethod(keyType, valueType);

						MethodInfo indexer = type.GetMethod("GetPair", BindingFlags.Public | BindingFlags.Instance)!;

						MethodInfo implicitConversion = typeof(AssetPair<,>).MakeGenericType(keyType, valueType).GetMethod("op_Implicit")!;

						int i = 0;
						while (true)
						{
							object pair = indexer.Invoke(value, [i])!;
							object keyValuePair = implicitConversion.Invoke(null, [pair])!;
							if (EnterDictionaryPair(walker, keyType, valueType, keyValuePair))
							{
								VisitValue(walker, walkType, keyType, pair.GetType().GetProperty(nameof(AssetPair<int, int>.Key))?.GetValue(pair)!);
								DivideDictionaryPair(walker, keyType, valueType, keyValuePair);
								VisitValue(walker, walkType, valueType, pair.GetType().GetProperty(nameof(AssetPair<int, int>.Value))?.GetValue(pair)!);
								ExitDictionaryPair(walker, keyType, valueType, keyValuePair);
							}
							i++;
							if (i >= count)
							{
								break;
							}
							divideDictionaryMethod.Invoke(walker, [value]);
						}
					}
					ExitDictionary(walker, keyType, valueType, value);
				}
			}
			else if (genericTypeDefinition == typeof(AssetPair<,>))
			{
				Type keyType = type.GetGenericArguments()[0];
				Type valueType = type.GetGenericArguments()[1];

				object? keyValuePair = type.GetMethod("op_Implicit")?.Invoke(null, [value])!;
				if (EnterPair(walker, keyType, valueType, keyValuePair))
				{
					VisitValue(walker, walkType, keyType, type.GetProperty(nameof(AssetPair<int, int>.Key))?.GetValue(value)!);
					DividePair(walker, keyType, valueType, keyValuePair);
					VisitValue(walker, walkType, valueType, type.GetProperty(nameof(AssetPair<int, int>.Value))?.GetValue(value)!);
					ExitPair(walker, keyType, valueType, keyValuePair);
				}
			}
			else
			{
				throw new NotImplementedException(type.Name);
			}
		}
		else if (type.IsAssignableTo(typeof(IUnityAssetBase)))
		{
			string methodName = walkType switch
			{
				WalkType.Editor => nameof(IUnityAssetBase.WalkEditor),
				WalkType.Release => nameof(IUnityAssetBase.WalkRelease),
				_ => nameof(IUnityAssetBase.WalkStandard),
			};
			typeof(IUnityAssetBase).GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance)
				!.Invoke(value, [walker]);
		}
		else
		{
			throw new NotSupportedException($"Unsupported type: {type.Name}");
		}
	}

	private static string GetName(FieldInfo field, WalkType walkType)
	{
		return walkType switch
		{
			WalkType.Editor => field.GetCustomAttribute<EditorFieldAttribute>()?.Name,
			WalkType.Release => field.GetCustomAttribute<ReleaseFieldAttribute>()?.Name,
			_ => null,
		} ?? field.Name;
	}
}
