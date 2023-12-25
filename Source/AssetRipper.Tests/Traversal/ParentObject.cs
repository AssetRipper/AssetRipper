using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Subclasses.Vector2f;

namespace AssetRipper.Tests.Traversal;

internal sealed class ParentObject : CustomInjectedObjectBase
{
	private readonly Vector2f coordinates = new();

	public const string Yaml = """
		%YAML 1.1
		%TAG !u! tag:unity3d.com,2011:
		--- !u!0 &1
		ParentObject:
		  coordinates: {x: 0, y: 0}

		""";

	public ParentObject(AssetInfo assetInfo) : base(assetInfo)
	{
	}
}
