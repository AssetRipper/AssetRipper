using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class AnimatorCondition : IYAMLExportable
	{
		public AnimatorCondition(AnimatorController controller, ConditionConstant condition)
		{
			ConditionMode = condition.ConditionMode;
			ConditionEvent = controller.TOS[condition.EventID];
			EventTreshold = condition.EventThreshold;
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_ConditionMode", (int)ConditionMode);
			node.Add("m_ConditionEvent", ConditionEvent);
			node.Add("m_EventTreshold", EventTreshold);
			return node;
		}

		public AnimatorConditionMode ConditionMode { get; private set; }
		public string ConditionEvent { get; private set; }
		public float EventTreshold { get; private set; }
	}
}
