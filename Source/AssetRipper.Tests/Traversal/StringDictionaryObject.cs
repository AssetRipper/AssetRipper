using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.UnityTexEnv;

namespace AssetRipper.Tests.Traversal;

internal sealed class StringDictionaryObject : CustomInjectedObjectBase
{
	public readonly AssetDictionary<Utf8String, Utf8String> stringDictionary = new() { { "key1", "value1" }, { "key2", "value2" } };
	private readonly AssetDictionary<Utf8String, UnityTexEnv_5> normalDictionary = new();

	public const string Yaml = """
		%YAML 1.1
		%TAG !u! tag:unity3d.com,2011:
		--- !u!0 &1
		StringDictionaryObject:
		  stringDictionary:
		  - key1: value1
		  - key2: value2
		  normalDictionary:
		  - _BumpMap:
		      m_Texture: {m_FileID: 0, m_PathID: 0}
		      m_Scale: {x: 1, y: 1}
		      m_Offset: {x: 0, y: 0}
		  - _DetailAlbedoMap:
		      m_Texture: {m_FileID: 0, m_PathID: 0}
		      m_Scale: {x: 1, y: 1}
		      m_Offset: {x: 0, y: 0}

		""";

	public const string YamlWithoutHyphens = """
		%YAML 1.1
		%TAG !u! tag:unity3d.com,2011:
		--- !u!0 &1
		StringDictionaryObject:
		  stringDictionary:
		    key1: value1
		    key2: value2
		  normalDictionary:
		    _BumpMap:
		      m_Texture: {m_FileID: 0, m_PathID: 0}
		      m_Scale: {x: 1, y: 1}
		      m_Offset: {x: 0, y: 0}
		    _DetailAlbedoMap:
		      m_Texture: {m_FileID: 0, m_PathID: 0}
		      m_Scale: {x: 1, y: 1}
		      m_Offset: {x: 0, y: 0}

		""";

	public StringDictionaryObject(AssetInfo assetInfo) : base(assetInfo)
	{
		{
			AssetPair<Utf8String, UnityTexEnv_5> pair = normalDictionary.AddNew();
			pair.Key = "_BumpMap";
			pair.Value.Scale.SetOne();
		}
		{
			AssetPair<Utf8String, UnityTexEnv_5> pair = normalDictionary.AddNew();
			pair.Key = "_DetailAlbedoMap";
			pair.Value.Scale.SetOne();
		}
	}
}
