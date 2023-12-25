using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Subclasses.StaticBatchInfo;

namespace AssetRipper.Tests.Traversal;

internal sealed class SubclassObject : CustomInjectedObjectBase
{
	private readonly StaticBatchInfo m_StaticBatchInfo = new();

	public const string Yaml = """
		%YAML 1.1
		%TAG !u! tag:unity3d.com,2011:
		--- !u!0 &1
		SubclassObject:
		  m_StaticBatchInfo:
		    firstSubMesh: 0
		    subMeshCount: 0

		""";

	public SubclassObject(AssetInfo assetInfo) : base(assetInfo)
	{
	}
}
