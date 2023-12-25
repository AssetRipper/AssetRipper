using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.Primitives;

namespace AssetRipper.Tests.Traversal;

internal sealed class PairListObject : CustomInjectedObjectBase
{
	private readonly AssetList<AssetPair<Utf8String, Utf8String>> list = new();

	public const string Yaml = """
		%YAML 1.1
		%TAG !u! tag:unity3d.com,2011:
		--- !u!0 &1
		PairListObject:
		  list:
		  - first: 
		    second: 
		  - first: _key
		    second: _value

		""";

	public PairListObject(AssetInfo assetInfo) : base(assetInfo)
	{
		list.AddNew();
		AssetPair<Utf8String, Utf8String> pair = list.AddNew();
		pair.Key = "_key";
		pair.Value = "_value";
	}
}
