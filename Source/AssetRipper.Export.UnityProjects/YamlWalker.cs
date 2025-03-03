using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.ExposedReferenceTable;
using AssetRipper.SourceGenerated.Subclasses.GUID;
using AssetRipper.SourceGenerated.Subclasses.Hash128;
using AssetRipper.SourceGenerated.Subclasses.PropertyName;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AssetRipper.Export.UnityProjects;

public class YamlWalker : AssetWalker
{
	private readonly record struct YamlContext(YamlMappingNode? MappingNode, YamlSequenceNode? SequenceNode, string? FieldName)
	{
		public YamlContext(YamlMappingNode mappingNode) : this(mappingNode, null, null)
		{
		}

		public YamlContext(YamlMappingNode mappingNode, string fieldName) : this(mappingNode, null, fieldName)
		{
		}

		public YamlContext(YamlSequenceNode sequenceNode) : this(null, sequenceNode, null)
		{
		}
	}

	/// <summary>
	/// These fields are excluded from meta files even though they might not have flags indicating that.
	/// </summary>
	/// <remarks>
	/// This set uses original names for robustness and clarity.
	/// </remarks>
	private static readonly HashSet<string> FieldsToSkipInImporters = new()
	{
		"m_ObjectHideFlags",
		"m_ExtensionPtr",
		"m_PrefabParentObject",
		"m_CorrespondingSourceObject",
		"m_PrefabInternal",
		"m_PrefabAsset",
		"m_PrefabInstance",
	};

	private const string First = "first";
	private const string Second = "second";
	private Stack<YamlContext> ContextStack { get; } = new();
	public bool ExportingAssetImporter { private get; set; }
	private YamlMappingNode? CurrentMappingNode => ContextStack.Peek().MappingNode;
	private YamlSequenceNode? CurrentSequenceNode => ContextStack.Peek().SequenceNode;
	private string? CurrentFieldName => ContextStack.Peek().FieldName;

	public YamlDocument ExportYamlDocument(IUnityObjectBase asset, long exportID)
	{
		ContextStack.Clear();

		YamlDocument document = new();

		YamlMappingNode root = document.CreateMappingRoot();
		root.Tag = asset.ClassID.ToString();
		root.Anchor = exportID.ToString();

		ContextStack.Push(new(root, null, asset.ClassName));
		asset.WalkEditor(this);

		if (asset.IsStripped())
		{
			Debug.Assert(root.Children.Count == 1);
			asset.RemoveStrippedFields((YamlMappingNode)root.Children[0].Value);
			root.Stripped = true;
		}

		return document;
	}

	public YamlNode ExportYamlNode(IUnityAssetBase asset)
	{
		ContextStack.Clear();
		YamlSequenceNode falseRoot = new();
		ContextStack.Push(new(falseRoot));
		asset.WalkEditor(this);
		Debug.Assert(falseRoot.Children.Count == 1);
		return falseRoot.Children[0];
	}

	public override bool EnterAsset(IUnityAssetBase asset)
	{
		if (asset is GUID guid)
		{
			VisitPrimitive(guid.ToString());
			return false;
		}
		else if (asset is IHash128 hash)
		{
			AddNode(HashHelper.ExportYaml(hash));
			return false;
		}
		else if (asset is IPropertyName propertyName)
		{
			VisitPrimitive(propertyName.GetIdString());
			return false;
		}
		else
		{
			bool result = EnterMap(asset.FlowMappedInYaml);
			Debug.Assert(result);
			Debug.Assert(CurrentMappingNode is { Children.Count: 0 });
			Debug.Assert(CurrentSequenceNode is null);
			Debug.Assert(CurrentFieldName is null);
			CurrentMappingNode.AddSerializedVersion(asset.SerializedVersion);
			return result;
		}
	}

	public sealed override void DivideAsset(IUnityAssetBase asset)
	{
	}

	public override void ExitAsset(IUnityAssetBase asset)
	{
		ExitMap();
	}

	public override bool EnterField(IUnityAssetBase asset, string name)
	{
		Debug.Assert(CurrentMappingNode is not null);
		Debug.Assert(CurrentSequenceNode is null);
		Debug.Assert(CurrentFieldName is null);
		if (ExportingAssetImporter && (FieldsToSkipInImporters.Contains(name) || asset.IgnoreFieldInMetaFiles(name)))
		{
			return false;
		}
		ContextStack.Push(new(CurrentMappingNode, name));
		return true;
	}

	public override void ExitField(IUnityAssetBase asset, string name)
	{
		Debug.Assert(CurrentMappingNode is not null);
		Debug.Assert(CurrentSequenceNode is null);
		Debug.Assert(CurrentFieldName is null);
		if (name is "m_Structure" && asset is IMonoBehaviour)
		{
			YamlMappingNode structureNode = PopNode(CurrentMappingNode.Children, "m_Structure");
			CurrentMappingNode.Append(structureNode);
		}
		if (name is "m_References" && asset is IExposedReferenceTable)
		{		
			YamlMappingNode referencesNode = PopNode(CurrentMappingNode.Children, "m_References");
			YamlSequenceNode sequenceNode = new();			
			foreach (KeyValuePair<YamlNode, YamlNode> child in referencesNode.Children)
			{
				YamlMappingNode childNode = new YamlMappingNode();
				childNode.Add(child.Key, child.Value);
				sequenceNode.Add(childNode);
			}
			CurrentMappingNode.Add("m_References", sequenceNode);			
		}
		static YamlMappingNode PopNode(List<KeyValuePair<YamlNode, YamlNode>> children, string key)
		{
			KeyValuePair<YamlNode, YamlNode> pair = children[^1];
			Debug.Assert(pair.Key.ToString() == key);
			children.RemoveAt(children.Count - 1);
			return (YamlMappingNode)pair.Value;
		}
	}

	public override bool EnterList<T>(IReadOnlyList<T> list)
	{
		// Integer arrays and lists are emitted as hex strings by Unity, both in custom MonoBehaviour fields and internal engine classes.
		// In this particular case, integer means the traditional C# integer types, bool, and char.
		// float and double are not emitted as hex strings. They are emitted as regular sequences.
		if (typeof(T) == typeof(sbyte) ||
			typeof(T) == typeof(byte) ||
			typeof(T) == typeof(short) ||
			typeof(T) == typeof(ushort) ||
			typeof(T) == typeof(int) ||
			typeof(T) == typeof(uint) ||
			typeof(T) == typeof(long) ||
			typeof(T) == typeof(ulong) ||
			typeof(T) == typeof(bool) ||
			typeof(T) == typeof(char))
		{
			VisitPrimitive(list);
			return false;
		}
		else
		{
			return EnterSequence(SequenceStyle.Block);
		}
	}

	public override void ExitList<T>(IReadOnlyList<T> list)
	{
		ExitSequence();
	}

	public override bool EnterPair<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
	{
		YamlMappingNode node = new YamlMappingNode();
		ContextStack.Push(new(node));
		if (!IsValidDictionaryKey<TKey>())
		{
			ContextStack.Push(new(node, First));
		}
		return true;
	}

	public override void DividePair<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
	{
		Debug.Assert(CurrentMappingNode is not null);
		Debug.Assert(CurrentSequenceNode is null);
		if (IsValidDictionaryKey<TKey>())
		{
			Debug.Assert(CurrentFieldName is not null);
		}
		else
		{
			Debug.Assert(CurrentFieldName is null);
			ContextStack.Push(new(CurrentMappingNode, Second));
		}
	}

	public override void ExitPair<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
	{
		ExitMap();
	}

	public override bool EnterDictionary<TKey, TValue>(IReadOnlyCollection<KeyValuePair<TKey, TValue>> dictionary)
	{
		if (IsValidDictionaryKey<TKey>())
		{
			return EnterMap();
		}
		else
		{
			return EnterSequence(SequenceStyle.BlockCurve);
		}
	}

	public override void ExitDictionary<TKey, TValue>(IReadOnlyCollection<KeyValuePair<TKey, TValue>> dictionary)
	{
		if (IsValidDictionaryKey<TKey>())
		{
			ExitMap();
		}
		else
		{
			ExitSequence();
		}
	}

	public override bool EnterDictionaryPair<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
	{
		if (IsValidDictionaryKey<TKey>())
		{
			Debug.Assert(CurrentMappingNode is not null);
			Debug.Assert(CurrentSequenceNode is null);
			Debug.Assert(CurrentFieldName is null);
		}
		else
		{
			Debug.Assert(CurrentMappingNode is null);
			Debug.Assert(CurrentSequenceNode is not null);
			Debug.Assert(CurrentFieldName is null);
			YamlMappingNode node = new();
			CurrentSequenceNode.Add(node);
			ContextStack.Push(new(node));
			ContextStack.Push(new(node, First));
		}
		return true;
	}

	public override void DivideDictionaryPair<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
	{
		if (IsValidDictionaryKey<TKey>())
		{
		}
		else
		{
			Debug.Assert(CurrentMappingNode is not null);
			Debug.Assert(CurrentSequenceNode is null);
			Debug.Assert(CurrentFieldName is null);
			ContextStack.Push(new(CurrentMappingNode, Second));
		}
	}

	public override void ExitDictionaryPair<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
	{
		if (IsValidDictionaryKey<TKey>())
		{
		}
		else
		{
			Debug.Assert(CurrentMappingNode is not null);
			Debug.Assert(CurrentSequenceNode is null);
			Debug.Assert(CurrentFieldName is null);
			ContextStack.Pop();
		}
	}

	public override void VisitPrimitive<T>(T value)
	{
		if (CurrentMappingNode is not null)
		{
			Debug.Assert(CurrentSequenceNode is null);

			if (CurrentFieldName is not null)
			{
				CurrentMappingNode.Add(CurrentFieldName, ToNode(value));

				//Pop the context off the stack
				ContextStack.Pop();
			}
			else
			{
				Debug.Assert(IsValidDictionaryKey<T>());
				ContextStack.Push(new(CurrentMappingNode, value?.ToString() ?? ""));
			}
		}
		else
		{
			Debug.Assert(CurrentSequenceNode is not null);
			Debug.Assert(CurrentFieldName is null);

			//Add to sequence
			CurrentSequenceNode.Add(ToNode(value));
		}

		static YamlNode ToNode(T value)
		{
			// IReadOnlyList<T> branches are from EnterList
			if (typeof(T) == typeof(IReadOnlyList<sbyte>))
			{
				return YamlScalarNode.CreateHex((IReadOnlyList<sbyte>)value);
			}
			else if (typeof(T) == typeof(IReadOnlyList<byte>))
			{
				return YamlScalarNode.CreateHex((IReadOnlyList<byte>)value);
			}
			else if (typeof(T) == typeof(IReadOnlyList<short>))
			{
				return YamlScalarNode.CreateHex((IReadOnlyList<short>)value);
			}
			else if (typeof(T) == typeof(IReadOnlyList<ushort>))
			{
				return YamlScalarNode.CreateHex((IReadOnlyList<ushort>)value);
			}
			else if (typeof(T) == typeof(IReadOnlyList<int>))
			{
				return YamlScalarNode.CreateHex((IReadOnlyList<int>)value);
			}
			else if (typeof(T) == typeof(IReadOnlyList<uint>))
			{
				return YamlScalarNode.CreateHex((IReadOnlyList<uint>)value);
			}
			else if (typeof(T) == typeof(IReadOnlyList<long>))
			{
				return YamlScalarNode.CreateHex((IReadOnlyList<long>)value);
			}
			else if (typeof(T) == typeof(IReadOnlyList<ulong>))
			{
				return YamlScalarNode.CreateHex((IReadOnlyList<ulong>)value);
			}
			else if (typeof(T) == typeof(IReadOnlyList<bool>))
			{
				return YamlScalarNode.CreateHex((IReadOnlyList<bool>)value);
			}
			else if (typeof(T) == typeof(IReadOnlyList<char>))
			{
				return YamlScalarNode.CreateHex((IReadOnlyList<char>)value);
			}
			else if (typeof(T) == typeof(byte[]))
			{
				return YamlScalarNode.CreateHex(Unsafe.As<T, byte[]>(ref value));
			}
			else if (typeof(T) == typeof(bool))
			{
				return YamlScalarNode.Create(Unsafe.As<T, bool>(ref value));
			}
			else if (typeof(T) == typeof(char))
			{
				return YamlScalarNode.Create(Unsafe.As<T, char>(ref value));
			}
			else if (typeof(T) == typeof(sbyte))
			{
				return YamlScalarNode.Create(Unsafe.As<T, sbyte>(ref value));
			}
			else if (typeof(T) == typeof(byte))
			{
				return YamlScalarNode.Create(Unsafe.As<T, byte>(ref value));
			}
			else if (typeof(T) == typeof(short))
			{
				return YamlScalarNode.Create(Unsafe.As<T, short>(ref value));
			}
			else if (typeof(T) == typeof(ushort))
			{
				return YamlScalarNode.Create(Unsafe.As<T, ushort>(ref value));
			}
			else if (typeof(T) == typeof(int))
			{
				return YamlScalarNode.Create(Unsafe.As<T, int>(ref value));
			}
			else if (typeof(T) == typeof(uint))
			{
				return YamlScalarNode.Create(Unsafe.As<T, uint>(ref value));
			}
			else if (typeof(T) == typeof(long))
			{
				return YamlScalarNode.Create(Unsafe.As<T, long>(ref value));
			}
			else if (typeof(T) == typeof(ulong))
			{
				return YamlScalarNode.Create(Unsafe.As<T, ulong>(ref value));
			}
			else if (typeof(T) == typeof(float))
			{
				return YamlScalarNode.Create(Unsafe.As<T, float>(ref value));
			}
			else if (typeof(T) == typeof(double))
			{
				return YamlScalarNode.Create(Unsafe.As<T, double>(ref value));
			}
			else if (typeof(T) == typeof(string))
			{
				return YamlScalarNode.Create(Unsafe.As<T, string>(ref value));
			}
			else if (typeof(T) == typeof(Utf8String))
			{
				return YamlScalarNode.Create(Unsafe.As<T, Utf8String>(ref value));
			}
			else
			{
				return YamlScalarNode.Create(value?.ToString() ?? "");// Fallback
			}
		}
	}

	public sealed override void VisitPPtr<TAsset>(PPtr<TAsset> pptr)
	{
		AddNode(CreateYamlNodeForPPtr(pptr));
	}

	public virtual YamlNode CreateYamlNodeForPPtr<TAsset>(PPtr<TAsset> pptr) where TAsset : IUnityObjectBase
	{
		YamlMappingNode mappingNode = new()
		{
			Style = MappingStyle.Flow,
		};
		mappingNode.Add("m_FileID", pptr.FileID);
		mappingNode.Add("m_PathID", pptr.PathID);
		mappingNode.Add("m_TargetClassID", GetClassID(typeof(TAsset)));
		return mappingNode;
	}

	protected static int GetClassID(Type type)
	{
		return (int)(ClassIDTypeMap.Dictionary.GetValueOrDefault(type, ClassIDType.Object));
	}

	private bool EnterMap(bool flowmapped = false)
	{
		ContextStack.Push(new(new YamlMappingNode(flowmapped ? MappingStyle.Flow : MappingStyle.Block)));
		return true;
	}

	private void ExitMap()
	{
		YamlContext context = ContextStack.Pop();
		Debug.Assert(context.MappingNode is not null);
		Debug.Assert(context.SequenceNode is null);
		Debug.Assert(context.FieldName is null);

		AddNode(context.MappingNode);
	}

	private bool EnterSequence(SequenceStyle style)
	{
		ContextStack.Push(new(new YamlSequenceNode(style)));
		return true;
	}

	private void ExitSequence()
	{
		YamlContext context = ContextStack.Pop();
		Debug.Assert(context.MappingNode is null);
		Debug.Assert(context.SequenceNode is not null);
		Debug.Assert(context.FieldName is null);

		AddNode(context.SequenceNode);
	}

	protected void AddNode(YamlNode node)
	{
		if (CurrentMappingNode is not null)
		{
			Debug.Assert(CurrentSequenceNode is null);
			Debug.Assert(CurrentFieldName is not null);
			CurrentMappingNode.Add(CurrentFieldName, node);
			ContextStack.Pop();
		}
		else
		{
			Debug.Assert(CurrentSequenceNode is not null);
			Debug.Assert(CurrentFieldName is null);
			CurrentSequenceNode.Add(node);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static bool IsString<T>() => typeof(T) == typeof(string) || typeof(T) == typeof(Utf8String);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static bool IsValidDictionaryKey<T>() => IsString<T>() || typeof(T).IsPrimitive || typeof(T) == typeof(GUID);

	private static class HashHelper
	{
		public static YamlMappingNode ExportYaml(IHash128 hash)
		{
			return ExportYaml(hash.Bytes__0, hash.Bytes__1, hash.Bytes__2, hash.Bytes__3, hash.Bytes__4, hash.Bytes__5, hash.Bytes__6, hash.Bytes__7, hash.Bytes__8, hash.Bytes__9, hash.Bytes_10, hash.Bytes_11, hash.Bytes_12, hash.Bytes_13, hash.Bytes_14, hash.Bytes_15, hash.SerializedVersion);
		}

		private static YamlMappingNode ExportYaml(byte bytes__0, byte bytes__1, byte bytes__2, byte bytes__3, byte bytes__4, byte bytes__5, byte bytes__6, byte bytes__7, byte bytes__8, byte bytes__9, byte bytes_10, byte bytes_11, byte bytes_12, byte bytes_13, byte bytes_14, byte bytes_15, int serializedVersion)
		{
			YamlMappingNode node = new();
			node.AddSerializedVersion(serializedVersion);
			if (serializedVersion > 1)
			{
				//Unity is stupid and didn't change the type trees.
				//To see an example of this, look at Texture3D.
				//This change happened at the beginning of Unity 5.
				string str = ToString(bytes__0, bytes__1, bytes__2, bytes__3, bytes__4, bytes__5, bytes__6, bytes__7, bytes__8, bytes__9, bytes_10, bytes_11, bytes_12, bytes_13, bytes_14, bytes_15);
				node.Add(HashName, str);
			}
			else
			{
				node.Add(Bytes00Name, bytes__0);
				node.Add(Bytes01Name, bytes__1);
				node.Add(Bytes02Name, bytes__2);
				node.Add(Bytes03Name, bytes__3);
				node.Add(Bytes04Name, bytes__4);
				node.Add(Bytes05Name, bytes__5);
				node.Add(Bytes06Name, bytes__6);
				node.Add(Bytes07Name, bytes__7);
				node.Add(Bytes08Name, bytes__8);
				node.Add(Bytes09Name, bytes__9);
				node.Add(Bytes10Name, bytes_10);
				node.Add(Bytes11Name, bytes_11);
				node.Add(Bytes12Name, bytes_12);
				node.Add(Bytes13Name, bytes_13);
				node.Add(Bytes14Name, bytes_14);
				node.Add(Bytes15Name, bytes_15);
			}
			return node;
		}

		public static string ToString(byte bytes__0, byte bytes__1, byte bytes__2, byte bytes__3, byte bytes__4, byte bytes__5, byte bytes__6, byte bytes__7, byte bytes__8, byte bytes__9, byte bytes_10, byte bytes_11, byte bytes_12, byte bytes_13, byte bytes_14, byte bytes_15)
		{
			//Not sure if this depends on Endianess
			//If it does, it might be best to split Hash at Unity 5
			uint Data0 = bytes__0 | (uint)bytes__1 << 8 | (uint)bytes__2 << 16 | (uint)bytes__3 << 24;
			uint Data1 = bytes__4 | (uint)bytes__5 << 8 | (uint)bytes__6 << 16 | (uint)bytes__7 << 24;
			uint Data2 = bytes__8 | (uint)bytes__9 << 8 | (uint)bytes_10 << 16 | (uint)bytes_11 << 24;
			uint Data3 = bytes_12 | (uint)bytes_13 << 8 | (uint)bytes_14 << 16 | (uint)bytes_15 << 24;
			string str = $"{Data0:x8}{Data1:x8}{Data2:x8}{Data3:x8}";
			return str;
		}

		private const string Bytes00Name = "bytes[0]";
		private const string Bytes01Name = "bytes[1]";
		private const string Bytes02Name = "bytes[2]";
		private const string Bytes03Name = "bytes[3]";
		private const string Bytes04Name = "bytes[4]";
		private const string Bytes05Name = "bytes[5]";
		private const string Bytes06Name = "bytes[6]";
		private const string Bytes07Name = "bytes[7]";
		private const string Bytes08Name = "bytes[8]";
		private const string Bytes09Name = "bytes[9]";
		private const string Bytes10Name = "bytes[10]";
		private const string Bytes11Name = "bytes[11]";
		private const string Bytes12Name = "bytes[12]";
		private const string Bytes13Name = "bytes[13]";
		private const string Bytes14Name = "bytes[14]";
		private const string Bytes15Name = "bytes[15]";
		private const string HashName = "Hash";
	}
}
