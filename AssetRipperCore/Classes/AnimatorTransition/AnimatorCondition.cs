using AssetRipper.Core.Classes.AnimatorController.Constants;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.AnimatorTransition
{
	public sealed class AnimatorCondition : IYamlExportable
	{
		public AnimatorCondition(ConditionConstant condition, IReadOnlyDictionary<uint, string> tos)
		{
			ConditionMode = condition.ConditionMode;
			ConditionEvent = tos[condition.EventID];
			EventTreshold = condition.EventThreshold;
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(ConditionModeName, (int)ConditionMode);
			node.Add(ConditionEventName, ConditionEvent);
			node.Add(EventTresholdName, EventTreshold);
			return node;
		}

		public AnimatorConditionMode ConditionMode { get; set; }
		public string ConditionEvent { get; set; }
		public float EventTreshold { get; set; }

		public const string ConditionModeName = "m_ConditionMode";
		public const string ConditionEventName = "m_ConditionEvent";
		public const string EventTresholdName = "m_EventTreshold";
	}
}
