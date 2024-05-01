using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Subclasses.ComponentPair;

namespace AssetRipper.Tests.Traversal;

internal sealed class ComponentListObject : CustomInjectedObjectBase
{
	private readonly AssetList<ComponentPair_5_5> m_Component = new();

	public const string Yaml = """
		%YAML 1.1
		%TAG !u! tag:unity3d.com,2011:
		--- !u!0 &1
		ComponentListObject:
		  m_Component:
		  - component: {m_FileID: 0, m_PathID: 0}
		  - component: {m_FileID: 0, m_PathID: 0}
		  - component: {m_FileID: 0, m_PathID: 0}
		  - component: {m_FileID: 0, m_PathID: 0}

		""";

	public ComponentListObject(AssetInfo assetInfo) : base(assetInfo)
	{
		m_Component.AddNew();
		m_Component.AddNew();
		m_Component.AddNew();
		m_Component.AddNew();
	}
}
