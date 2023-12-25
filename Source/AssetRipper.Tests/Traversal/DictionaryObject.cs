using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Subclasses.Vector3f;

namespace AssetRipper.Tests.Traversal;

internal sealed class DictionaryObject : CustomInjectedObjectBase
{
	private readonly AssetDictionary<Utf8String, Utf8String> emptyDictionary = [];
	private readonly AssetDictionary<Vector3f, int> vectorDictionary = new();

	public const string Yaml = """
		%YAML 1.1
		%TAG !u! tag:unity3d.com,2011:
		--- !u!0 &1
		DictionaryObject:
		  emptyDictionary: {}
		  vectorDictionary:
		  - first: {x: 0, y: 0, z: 0}
		    second: 0
		  - first: {x: 0, y: 0, z: 0}
		    second: 0

		""";

	public DictionaryObject(AssetInfo assetInfo) : base(assetInfo)
	{
		vectorDictionary.AddNew();
		vectorDictionary.AddNew();
	}
}
