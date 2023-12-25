using AssetRipper.Assets.Metadata;

namespace AssetRipper.Tests.Traversal;

internal sealed class SerializedVersionObject : CustomInjectedObjectBase
{
	public override int SerializedVersion => 3;

	public const string Yaml = """
		%YAML 1.1
		%TAG !u! tag:unity3d.com,2011:
		--- !u!0 &1
		SerializedVersionObject:
		  serializedVersion: 3

		""";

	private SerializedVersionObject(AssetInfo assetInfo) : base(assetInfo)
	{
	}
}
