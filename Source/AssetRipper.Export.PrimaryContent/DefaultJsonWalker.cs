using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.Primitives;
using System.CodeDom.Compiler;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Web;

namespace AssetRipper.Export.PrimaryContent;

public class DefaultJsonWalker : AssetWalker
{
	private readonly StringWriter stringWriter = new(CultureInfo.InvariantCulture) { NewLine = "\n" };
	protected IndentedTextWriter Writer { get; }

	public DefaultJsonWalker()
	{
		Writer = new IndentedTextWriter(stringWriter, "\t");
	}

	public string SerializeEditor<T>(T asset) where T : IUnityAssetBase
	{
		Clear();
		asset.WalkEditor(this);
		return stringWriter.ToString();
	}

	public string SerializeRelease<T>(T asset) where T : IUnityAssetBase
	{
		Clear();
		asset.WalkRelease(this);
		return stringWriter.ToString();
	}

	public string SerializeStandard<T>(T asset) where T : IUnityAssetBase
	{
		Clear();
		asset.WalkStandard(this);
		return stringWriter.ToString();
	}

	private void Clear()
	{
		Writer.Flush();
		stringWriter.GetStringBuilder().Clear();
	}

	public override bool EnterAsset(IUnityAssetBase asset)
	{
		Writer.WriteLine('{');
		Writer.Indent++;
		return true;
	}

	public override void DivideAsset(IUnityAssetBase asset)
	{
		Writer.WriteLine(',');
	}

	public override void ExitAsset(IUnityAssetBase asset)
	{
		Writer.WriteLine();
		Writer.Indent--;
		Writer.Write('}');
	}

	public override bool EnterField(IUnityAssetBase asset, string name)
	{
		Writer.Write(HttpUtility.JavaScriptStringEncode(name, true));
		Writer.Write(": ");
		return true;
	}

	public override bool EnterList<T>(AssetList<T> list)
	{
		Writer.WriteLine('[');
		Writer.Indent++;
		return true;
	}

	public override void DivideList<T>(AssetList<T> list)
	{
		Writer.WriteLine(',');
	}

	public override void ExitList<T>(AssetList<T> list)
	{
		Writer.WriteLine();
		Writer.Indent--;
		Writer.Write(']');
	}

	public override bool EnterArray<T>(T[] array)
	{
		Writer.WriteLine('[');
		Writer.Indent++;
		return true;
	}

	public override void DivideArray<T>(T[] array)
	{
		Writer.WriteLine(',');
	}

	public override void ExitArray<T>(T[] array)
	{
		Writer.WriteLine();
		Writer.Indent--;
		Writer.Write(']');
	}

	public override bool EnterDictionary<TKey, TValue>(AssetDictionary<TKey, TValue> dictionary)
	{
		if (IsString<TKey>())
		{
			Writer.WriteLine('{');
			Writer.Indent++;
		}
		else
		{
			Writer.WriteLine('[');
			Writer.Indent++;
		}
		return true;
	}

	public override void DivideDictionary<TKey, TValue>(AssetDictionary<TKey, TValue> dictionary)
	{
		Writer.WriteLine(',');
	}

	public override void ExitDictionary<TKey, TValue>(AssetDictionary<TKey, TValue> dictionary)
	{
		if (IsString<TKey>())
		{
			Writer.WriteLine();
			Writer.Indent--;
			Writer.Write('}');
		}
		else
		{
			Writer.WriteLine();
			Writer.Indent--;
			Writer.Write(']');
		}
	}

	public override bool EnterDictionaryPair<TKey, TValue>(AssetPair<TKey, TValue> pair)
	{
		if (IsString<TKey>())
		{
			return true;
		}
		else
		{
			return EnterPair(pair);
		}
	}

	public override void DivideDictionaryPair<TKey, TValue>(AssetPair<TKey, TValue> pair)
	{
		if (IsString<TKey>())
		{
			Writer.Write(": ");
		}
		else
		{
			DividePair(pair);
		}
	}

	public override void ExitDictionaryPair<TKey, TValue>(AssetPair<TKey, TValue> pair)
	{
		if (IsString<TKey>())
		{
		}
		else
		{
			ExitPair(pair);
		}
	}

	public override bool EnterPair<TKey, TValue>(AssetPair<TKey, TValue> pair)
	{
		Writer.WriteLine('{');
		Writer.Indent++;
		Writer.Write("\"Key\": ");
		return true;
	}

	public override void DividePair<TKey, TValue>(AssetPair<TKey, TValue> pair)
	{
		Writer.WriteLine(',');
		Writer.Write("\"Value\": ");
	}

	public override void ExitPair<TKey, TValue>(AssetPair<TKey, TValue> pair)
	{
		Writer.WriteLine();
		Writer.Indent--;
		Writer.Write('}');
	}

	public override void VisitPrimitive<T>(T value)
	{
		if (typeof(T) == typeof(byte[]))
		{
			Writer.Write('"');
			Writer.Write(Convert.ToBase64String(Unsafe.As<T, byte[]>(ref value), Base64FormattingOptions.None));
			Writer.Write('"');
		}
		else if (typeof(T) == typeof(string))
		{
			Writer.Write('"');
			Writer.Write(HttpUtility.JavaScriptStringEncode(Unsafe.As<T, string>(ref value)));
			Writer.Write('"');
		}
		else if (typeof(T) == typeof(Utf8String))
		{
			Writer.Write('"');
			Writer.Write(HttpUtility.JavaScriptStringEncode(Unsafe.As<T, Utf8String>(ref value)));
			Writer.Write('"');
		}
		else if (typeof(T) == typeof(bool))
		{
			Writer.Write(Unsafe.As<T, bool>(ref value) ? "true" : "false");
		}
		else if (typeof(T) == typeof(char))
		{
			Writer.Write('"');
			Writer.Write(Unsafe.As<T, char>(ref value));
			Writer.Write('"');
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
			Writer.Write('"');
			Writer.Write(HttpUtility.JavaScriptStringEncode(value?.ToString()));
			Writer.Write('"');
		}
	}

	public override void VisitPPtr<TAsset>(PPtr<TAsset> pptr)
	{
		Writer.Write("{ \"m_FileID\": ");
		Writer.Write(pptr.FileID);
		Writer.Write(", \"m_PathID\": ");
		Writer.Write(pptr.PathID);
		Writer.Write(" }");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static bool IsString<T>() => typeof(T) == typeof(string) || typeof(T) == typeof(Utf8String);
}
