using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class StateBehavioursPair : IYAMLExportable
	{
		public StateBehavioursPair(AnimatorState state, MonoBehaviour[] behaviours)
		{
			if(state == null)
			{
				throw new ArgumentNullException(nameof(state));
			}
			if (behaviours == null || behaviours.Length == 0)
			{
				throw new ArgumentNullException(nameof(behaviours));
			}

			State = state.File.CreatePPtr(state);
			
			m_stateMachineBehaviours = new PPtr<MonoBehaviour>[behaviours.Length];
			for(int i = 0; i < behaviours.Length; i++)
			{
				MonoBehaviour behaviour = behaviours[i];
				PPtr<MonoBehaviour> behaviourPtr = behaviour.File.CreatePPtr(behaviour);
				m_stateMachineBehaviours[i] = behaviourPtr;
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_State", State.ExportYAML(container));
			node.Add("m_StateMachineBehaviours", StateMachineBehaviours.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<PPtr<MonoBehaviour>> StateMachineBehaviours => m_stateMachineBehaviours;

		public PPtr<AnimatorState> State;

		private PPtr<MonoBehaviour>[] m_stateMachineBehaviours;
	}
}
