using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Subclasses.GUID;

namespace AssetRipper.Tests.Traversal;

internal sealed class GuidDictionaryObject : CustomInjectedObjectBase
{
	private readonly AssetDictionary<GUID, bool> guidDictionary = new();

	public const string Yaml = """
		%YAML 1.1
		%TAG !u! tag:unity3d.com,2011:
		--- !u!0 &1
		GuidDictionaryObject:
		  guidDictionary:
		  - 00000000000000000000000000000000: 0
		  - 00000000000000000000000000000000: 1

		""";

	public const string YamlWithoutHyphens = """
		%YAML 1.1
		%TAG !u! tag:unity3d.com,2011:
		--- !u!0 &1
		GuidDictionaryObject:
		  guidDictionary:
		    00000000000000000000000000000000: 0
		    00000000000000000000000000000000: 1

		""";

	public GuidDictionaryObject(AssetInfo assetInfo) : base(assetInfo)
	{
		guidDictionary.AddNew();
		guidDictionary.AddNew().Value = true;
	}
}
