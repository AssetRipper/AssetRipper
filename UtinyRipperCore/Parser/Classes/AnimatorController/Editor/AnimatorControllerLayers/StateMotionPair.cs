using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class StateMotionPair : IYAMLExportable
	{
		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_State", State.ExportYAML(container));
			node.Add("m_Motion", Motion.ExportYAML(container));
			return node;
		}

		public PPtr<AnimatorState> State;
		public PPtr<Motion> Motion;
	}
}
