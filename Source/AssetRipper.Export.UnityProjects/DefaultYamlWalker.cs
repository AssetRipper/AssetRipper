using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_2089858483;
using AssetRipper.SourceGenerated.Subclasses.GUID;
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace AssetRipper.Export.UnityProjects;

public class DefaultYamlWalker : AssetWalker
{
	protected IndentedTextWriter Writer { get; }
	protected bool FlowMapping { get; private set; }
	protected bool FirstField { get; private set; }
	protected bool JustEnteredListItem { get; private set; }
	protected bool JustEnteredMonoBehaviourStructure { get; private set; }
	protected virtual bool UseHyphenInStringDictionary => true;

	private const string Head = """
		%YAML 1.1
		%TAG !u! tag:unity3d.com,2011:
		""";
	private const string IndentString = "  ";

	public DefaultYamlWalker(TextWriter innerWriter)
	{
		Writer = new IndentedTextWriter(innerWriter, IndentString);
		WriteHead();
	}

	protected void WriteHead()
	{
		Writer.WriteLine(Head);
	}

	private void SetFlowMappingTrue()
	{
		ThrowIfFlowMapping();
		FlowMapping = true;
	}

	private void ThrowIfFlowMapping()
	{
		if (FlowMapping)
		{
			throw new InvalidOperationException();
		}
	}

	private void SetFlowMappingFalse()
	{
		if (!FlowMapping)
		{
			throw new InvalidOperationException();
		}
		FlowMapping = false;
	}

	public DefaultYamlWalker AppendEditor(IUnityObjectBase asset, long exportID)
	{
		WriteTagAnchorRoot(asset, exportID);
		asset.WalkEditor(this);
		Writer.WriteLine();
		return this;
	}

	public DefaultYamlWalker AppendRelease(IUnityObjectBase asset, long exportID)
	{
		WriteTagAnchorRoot(asset, exportID);
		asset.WalkRelease(this);
		Writer.WriteLine();
		return this;
	}

	public DefaultYamlWalker AppendStandard(IUnityObjectBase asset, long exportID)
	{
		WriteTagAnchorRoot(asset, exportID);
		asset.WalkStandard(this);
		Writer.WriteLine();
		return this;
	}

	private void WriteTagAnchorRoot(IUnityObjectBase asset, long exportID)
	{
		Writer.Write("--- !u!");
		Writer.Write(asset.ClassID);//Tag
		Writer.Write(" &");
		Writer.WriteLine(exportID);//Anchor
		Writer.Write(asset.ClassName);//Root
		Writer.Write(':');
	}

	public sealed override bool EnterAsset(IUnityAssetBase asset)
	{
		ThrowIfFlowMapping();
		if (asset is GUID guid)
		{
			Writer.Write(' ');
			Writer.Write(guid.ToString());
			JustEnteredListItem = false;
			return false;
		}
		else if (JustEnteredMonoBehaviourStructure)
		{
			JustEnteredMonoBehaviourStructure = false;
			return true;
		}
		if (asset.FlowMappedInYaml)
		{
			SetFlowMappingTrue();
			if (JustEnteredListItem)
			{
				JustEnteredListItem = false;
				Writer.Write("{");
			}
			else
			{
				Writer.Write(" {");
			}
		}
		else
		{
			FirstField = true;
			if (JustEnteredListItem)
			{
				JustEnteredListItem = false;
			}
			else
			{
				Writer.WriteLine();
				Writer.Write(IndentString);
			}
			if (asset.SerializedVersion > 1)
			{
				if (EnterField(asset, "serializedVersion"))
				{
					VisitPrimitive(asset.SerializedVersion);
					ExitField(asset, "serializedVersion");
				}
				DivideAsset(asset);
			}
		}
		return true;
	}

	public sealed override void DivideAsset(IUnityAssetBase asset)
	{
		if (FlowMapping)
		{
			Writer.Write(", ");
		}
	}

	public sealed override void ExitAsset(IUnityAssetBase asset)
	{
		if (asset.FlowMappedInYaml)
		{
			SetFlowMappingFalse();
			Writer.Write('}');
		}
		else
		{
			Writer.Indent--;
		}
	}

	public sealed override bool EnterField(IUnityAssetBase asset, string name)
	{
		if (name is "m_Structure" && asset is IMonoBehaviour or IScriptedImporter)
		{
			JustEnteredMonoBehaviourStructure = true;
			return true;
		}
		if (FlowMapping)
		{
			Writer.Write(name);
			Writer.Write(':');
		}
		else
		{
			if (FirstField)
			{
				Writer.Indent++;
			}
			else
			{
				Writer.WriteLine();
			}
			Writer.Write(name);
			Writer.Write(':');
		}
		FirstField = false;
		return true;
	}

	public sealed override void ExitField(IUnityAssetBase asset, string name)
	{
	}

	public sealed override bool EnterList<T>(AssetList<T> list)
	{
		ThrowIfFlowMapping();
		if (list.Count == 0)
		{
			Writer.Write(" []");
			return false;
		}
		else
		{
			DivideList(list);
			return true;
		}
	}

	public sealed override void DivideList<T>(AssetList<T> list)
	{
		Writer.WriteLine();
		Writer.Write("- ");
		JustEnteredListItem = true;
	}

	public sealed override void ExitList<T>(AssetList<T> list)
	{
	}

	public sealed override bool EnterArray<T>(T[] array)
	{
		ThrowIfFlowMapping();
		if (array.Length == 0)
		{
			Writer.Write(" []");
			return false;
		}
		else
		{
			DivideArray(array);
			return true;
		}
	}

	public sealed override void DivideArray<T>(T[] array)
	{
		Writer.WriteLine();
		Writer.Write("- ");
		JustEnteredListItem = true;
	}

	public sealed override void ExitArray<T>(T[] array)
	{
	}

	public sealed override bool EnterDictionary<TKey, TValue>(AssetDictionary<TKey, TValue> dictionary)
	{
		ThrowIfFlowMapping();
		if (dictionary.Count == 0)
		{
			Writer.Write(" {}");
			return false;
		}
		else
		{
			DivideDictionary(dictionary);
			return true;
		}
	}

	public sealed override void DivideDictionary<TKey, TValue>(AssetDictionary<TKey, TValue> dictionary)
	{
		if (IsStringLike<TKey>())
		{
			Writer.WriteLine();
		}
	}

	public sealed override void ExitDictionary<TKey, TValue>(AssetDictionary<TKey, TValue> dictionary)
	{
	}

	public sealed override bool EnterDictionaryPair<TKey, TValue>(AssetPair<TKey, TValue> pair)
	{
		ThrowIfFlowMapping();
		if (IsStringLike<TKey>())
		{
			//This is wrong. See https://github.com/AssetRipper/AssetRipper/issues/821
			Writer.Write(UseHyphenInStringDictionary ? '-' : ' ');
		}
		else
		{
			Writer.WriteLine();
			Writer.Write("- first:");
		}
		Writer.Indent++;
		return true;
	}

	public sealed override void DivideDictionaryPair<TKey, TValue>(AssetPair<TKey, TValue> pair)
	{
		if (IsStringLike<TKey>())
		{
			Writer.Write(':');
		}
		else
		{
			Writer.Indent--;
			Writer.WriteLine();
			Writer.Write("  second:");
			Writer.Indent++;
		}
	}

	public sealed override void ExitDictionaryPair<TKey, TValue>(AssetPair<TKey, TValue> pair)
	{
		Writer.Indent--;
	}

	public sealed override bool EnterPair<TKey, TValue>(AssetPair<TKey, TValue> pair)
	{
		ThrowIfFlowMapping();
		if (JustEnteredListItem)
		{
			JustEnteredListItem = false;
			Writer.Write("first:");
			Writer.Indent++;
		}
		else
		{
			Writer.WriteLine();
			Writer.Indent++;
			Writer.Write("first:");
		}
		return true;
	}

	public sealed override void DividePair<TKey, TValue>(AssetPair<TKey, TValue> pair)
	{
		Writer.WriteLine();
		Writer.Write("second:");
	}

	public sealed override void ExitPair<TKey, TValue>(AssetPair<TKey, TValue> pair)
	{
		Writer.Indent--;
	}

	public sealed override void VisitPrimitive<T>(T value)
	{
		if (JustEnteredListItem)
		{
			JustEnteredListItem = false;
		}
		else
		{
			Writer.Write(' ');
		}

		if (typeof(T) == typeof(byte[]))
		{
			//This uses uppercase hex digits, but it shouldn't matter.
			Writer.Write(Convert.ToHexString(Unsafe.As<T, byte[]>(ref value)));
		}
		else if (typeof(T) == typeof(string))
		{
			WriteString(Unsafe.As<T, string?>(ref value));
		}
		else if (typeof(T) == typeof(Utf8String))
		{
			WriteString(Unsafe.As<T, Utf8String?>(ref value));
		}
		else if (typeof(T) == typeof(bool))
		{
			Writer.Write(Unsafe.As<T, bool>(ref value) ? "1" : "0");//Yaml booleans are displayed as 1 and 0.
		}
		else if (typeof(T) == typeof(char))
		{
			Writer.Write(Unsafe.As<T, ushort>(ref value));//Might be signed, rather than unsigned.
		}
		else if (typeof(T) == typeof(sbyte))
		{
			Writer.Write(Unsafe.As<T, sbyte>(ref value));
		}
		else if (typeof(T) == typeof(byte))
		{
			Writer.Write(Unsafe.As<T, byte>(ref value));
		}
		else if (typeof(T) == typeof(short))
		{
			Writer.Write(Unsafe.As<T, short>(ref value));
		}
		else if (typeof(T) == typeof(ushort))
		{
			Writer.Write(Unsafe.As<T, ushort>(ref value));
		}
		else if (typeof(T) == typeof(int))
		{
			Writer.Write(Unsafe.As<T, int>(ref value));
		}
		else if (typeof(T) == typeof(uint))
		{
			Writer.Write(Unsafe.As<T, uint>(ref value));
		}
		else if (typeof(T) == typeof(long))
		{
			Writer.Write(Unsafe.As<T, long>(ref value));
		}
		else if (typeof(T) == typeof(ulong))
		{
			Writer.Write(Unsafe.As<T, ulong>(ref value));
		}
		else if (typeof(T) == typeof(float))
		{
			Writer.Write(Unsafe.As<T, float>(ref value));
		}
		else if (typeof(T) == typeof(double))
		{
			Writer.Write(Unsafe.As<T, double>(ref value));
		}
		else
		{
			WriteString(value?.ToString());
		}

		void WriteString(string? str)
		{
			string? escapedString = YamlEscaping.Escape(str);
			if (escapedString != str)
			{
				Writer.Write('"');
				Writer.Write(escapedString);
				Writer.Write('"');
			}
			else
			{
				Writer.Write(str);
			}
		}
	}

	public sealed override void VisitPPtr<TAsset>(PPtr<TAsset> pptr)
	{
		if (JustEnteredListItem)
		{
			JustEnteredListItem = false;
		}
		else
		{
			Writer.Write(' ');
		}

		WritePPtr(pptr);
	}

	public virtual void WritePPtr<TAsset>(PPtr<TAsset> pptr) where TAsset : IUnityObjectBase
	{
		Writer.Write("{m_FileID: ");
		Writer.Write(pptr.FileID);
		Writer.Write(", m_PathID: ");
		Writer.Write(pptr.PathID);
		Writer.Write('}');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static bool IsString<T>() => typeof(T) == typeof(string) || typeof(T) == typeof(Utf8String);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static bool IsStringLike<T>() => IsString<T>() || typeof(T) == typeof(GUID);
}
