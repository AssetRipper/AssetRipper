using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.Tests.Traversal;

internal sealed class ListObject : CustomInjectedObjectBase
{
	private readonly AssetList<ColorRGB> colorList = new();
	private readonly AssetList<VectorXY> vectorList = new();

	public const string Yaml = """
		%YAML 1.1
		%TAG !u! tag:unity3d.com,2011:
		--- !u!0 &1
		ListObject:
		  colorList:
		  - {r: 1, g: 0.5, b: 0}
		  - {r: 1, g: 0.5, b: 0}
		  vectorList:
		  - serializedVersion: 2
		    xy: 0
		  - serializedVersion: 2
		    xy: 0

		""";

	public ListObject(AssetInfo assetInfo) : base(assetInfo)
	{
		colorList.AddNew();
		colorList.AddNew();
		vectorList.AddNew();
		vectorList.AddNew();
	}
}
