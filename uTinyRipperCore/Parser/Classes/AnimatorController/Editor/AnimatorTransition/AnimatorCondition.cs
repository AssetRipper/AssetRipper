using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class AnimatorCondition : IYAMLExportable
	{
		public AnimatorCondition(ConditionConstant condition, IReadOnlyDictionary<uint, string> tos)
		{
			ConditionMode = condition.ConditionMode;
			ConditionEvent = tos[condition.EventID];
			EventTreshold = condition.EventThreshold;
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(ConditionModeName, (int)ConditionMode);
			node.Add(ConditionEventName, ConditionEvent);
			node.Add(EventTresholdName, EventTreshold);
			return node;
		}

		public AnimatorConditionMode ConditionMode { get; private set; }
		public string ConditionEvent { get; private set; }
		public float EventTreshold { get; private set; }

		public const string ConditionModeName = "m_ConditionMode";
		public const string ConditionEventName = "m_ConditionEvent";
		public const string EventTresholdName = "m_EventTreshold";
	}
}
