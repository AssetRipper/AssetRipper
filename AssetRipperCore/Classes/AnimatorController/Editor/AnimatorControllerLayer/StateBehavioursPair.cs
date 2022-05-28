using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.AnimatorController.Editor.AnimatorControllerLayer
{
	public sealed class StateBehavioursPair : IYamlExportable
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

			State = state.SerializedFile.CreatePPtr(state);

			StateMachineBehaviours = new PPtr<MonoBehaviour>[behaviours.Length];
			for (int i = 0; i < behaviours.Length; i++)
			{
				MonoBehaviour behaviour = behaviours[i];
				PPtr<MonoBehaviour> behaviourPtr = behaviour.SerializedFile.CreatePPtr(behaviour);
				StateMachineBehaviours[i] = behaviourPtr;
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(StateName, State.ExportYaml(container));
			node.Add(StateMachineBehavioursName, StateMachineBehaviours.ExportYaml(container));
			return node;
		}

		public PPtr<MonoBehaviour>[] StateMachineBehaviours { get; set; }

		public const string StateName = "m_State";
		public const string StateMachineBehavioursName = "m_StateMachineBehaviours";

		public PPtr<AnimatorState> State = new();
	}
}
