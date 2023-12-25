using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_43;

namespace AssetRipper.Tests.Traversal;

internal sealed class SimpleObject : CustomInjectedObjectBase
{
	private readonly bool boolean = true;
	private readonly string name = nameof(SimpleObject);
	private readonly int integer = 42;
	private readonly IMesh? mesh;

	public const string Yaml = """
		%YAML 1.1
		%TAG !u! tag:unity3d.com,2011:
		--- !u!0 &1
		SimpleObject:
		  boolean: 1
		  name: SimpleObject
		  integer: 42
		  mesh: {m_FileID: 0, m_PathID: 0}

		""";

	private SimpleObject(AssetInfo assetInfo) : base(assetInfo)
	{
	}
}
