using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class StateBehavioursPair : IYAMLExportable
	{
		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_State", State.ExportYAML(exporter));
			node.Add("m_StateMachineBehaviours", StateMachineBehaviours.ExportYAML(exporter));
			return node;
		}

		public IReadOnlyList<PPtr<MonoBehaviour>> StateMachineBehaviours => m_stateMachineBehaviours;

		public PPtr<AnimatorState> State;

		private PPtr<MonoBehaviour>[] m_stateMachineBehaviours;
	}
}
