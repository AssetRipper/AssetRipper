using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.Primitives;

namespace AssetRipper.Tests.Traversal;

internal sealed class PairObject : CustomInjectedObjectBase
{
	private readonly AssetPair<Utf8String, Utf8String> pair = new()
	{
		Key = "_key",
		Value = "_value",
	};

	public const string Yaml = """
		%YAML 1.1
		%TAG !u! tag:unity3d.com,2011:
		--- !u!0 &1
		PairObject:
		  pair:
		    first: _key
		    second: _value

		""";

	public PairObject(AssetInfo assetInfo) : base(assetInfo)
	{
	}
}
