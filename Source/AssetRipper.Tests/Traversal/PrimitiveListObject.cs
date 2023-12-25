using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.Tests.Traversal;

internal sealed class PrimitiveListObject : CustomInjectedObjectBase
{
	private readonly AssetList<int> emptyList = [];
	private readonly AssetList<int> list = [1, 1, 2, 3, 5];

	public const string Yaml = """
		%YAML 1.1
		%TAG !u! tag:unity3d.com,2011:
		--- !u!0 &1
		PrimitiveListObject:
		  emptyList: []
		  list:
		  - 1
		  - 1
		  - 2
		  - 3
		  - 5

		""";

	private PrimitiveListObject(AssetInfo assetInfo) : base(assetInfo)
	{
	}
}
