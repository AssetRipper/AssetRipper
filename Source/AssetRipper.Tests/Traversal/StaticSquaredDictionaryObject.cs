using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Subclasses.StaticBatchInfo;

namespace AssetRipper.Tests.Traversal;

internal sealed class StaticSquaredDictionaryObject : CustomInjectedObjectBase
{
	private readonly AssetDictionary<StaticBatchInfo, StaticBatchInfo> dictionary = new();

	public const string Yaml = """
		%YAML 1.1
		%TAG !u! tag:unity3d.com,2011:
		--- !u!0 &1
		StaticSquaredDictionaryObject:
		  dictionary:
		  - first:
		      firstSubMesh: 0
		      subMeshCount: 0
		    second:
		      firstSubMesh: 0
		      subMeshCount: 0
		  - first:
		      firstSubMesh: 0
		      subMeshCount: 0
		    second:
		      firstSubMesh: 0
		      subMeshCount: 0

		""";

	public StaticSquaredDictionaryObject(AssetInfo assetInfo) : base(assetInfo)
	{
		dictionary.AddNew();
		dictionary.AddNew();
	}
}
