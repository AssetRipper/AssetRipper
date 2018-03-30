using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class StateMotionPair : IYAMLExportable
	{
		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_State", State.ExportYAML(exporter));
			node.Add("m_Motion", Motion.ExportYAML(exporter));
			return node;
		}

		public PPtr<AnimatorState> State;
		public PPtr<Motion> Motion;
	}
}
