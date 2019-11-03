using System;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers
{
	public sealed class StateBehavioursPair : IYAMLExportable
	{
		public StateBehavioursPair(AnimatorState state, MonoBehaviour[] behaviours)
		{
			if (state == null)
			{
				throw new ArgumentNullException(nameof(state));
			}
			if (behaviours == null || behaviours.Length == 0)
			{
				throw new ArgumentNullException(nameof(behaviours));
			}

			State = state.File.CreatePPtr(state);

			StateMachineBehaviours = new PPtr<MonoBehaviour>[behaviours.Length];
			for (int i = 0; i < behaviours.Length; i++)
			{
				MonoBehaviour behaviour = behaviours[i];
				PPtr<MonoBehaviour> behaviourPtr = behaviour.File.CreatePPtr(behaviour);
				StateMachineBehaviours[i] = behaviourPtr;
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(StateName, State.ExportYAML(container));
			node.Add(StateMachineBehavioursName, StateMachineBehaviours.ExportYAML(container));
			return node;
		}

		public PPtr<MonoBehaviour>[] StateMachineBehaviours { get; set; }

		public const string StateName = "m_State";
		public const string StateMachineBehavioursName = "m_StateMachineBehaviours";

		public PPtr<AnimatorState> State;
	}
}
