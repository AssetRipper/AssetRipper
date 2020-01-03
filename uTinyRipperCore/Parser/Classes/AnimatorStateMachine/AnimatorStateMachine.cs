using System;
using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Classes.Objects;
using uTinyRipper.Converters;
using uTinyRipper.Classes.AnimatorControllers;
using uTinyRipper.Classes.AnimatorStateMachines;
using uTinyRipper.Layout;

namespace uTinyRipper.Classes
{
	public sealed class AnimatorStateMachine : NamedObject
	{
		private AnimatorStateMachine(AssetLayout layout, AssetInfo assetInfo, AnimatorController controller, int stateMachineIndex) :
			base(layout)
		{
			AssetInfo = assetInfo;
			ObjectHideFlags = HideFlags.HideInHierarchy;

			VirtualSerializedFile virtualFile = (VirtualSerializedFile)assetInfo.File;

			int layerIndex = controller.Controller.GetLayerIndexByStateMachineIndex(stateMachineIndex);
			ref LayerConstant layer = ref controller.Controller.LayerArray[layerIndex].Instance;
			Name = controller.TOS[layer.Binding];

			StateMachineConstant stateMachine = controller.Controller.StateMachineArray[stateMachineIndex].Instance;

			int stateCount = stateMachine.StateConstantArray.Length;
			int stateMachineCount = 0;
			int count = stateCount + stateMachineCount;
			int side = (int)Math.Ceiling(Math.Sqrt(count));

			List<AnimatorState> states = new List<AnimatorState>();
			ChildStates = new ChildAnimatorState[stateCount];
			for (int y = 0, stateIndex = 0; y < side && stateIndex < stateCount; y++)
			{
				for (int x = 0; x < side && stateIndex < stateCount; x++, stateIndex++)
				{
					Vector3f position = new Vector3f(x * StateOffset, y * StateOffset, 0.0f);
					AnimatorState state = AnimatorState.CreateVirtualInstance(virtualFile, controller, stateMachineIndex, stateIndex, position);
					ChildAnimatorState childState = new ChildAnimatorState(state, position);
					ChildStates[stateIndex] = childState;
					states.Add(state);
				}
			}
#warning TODO: child StateMachines
			ChildStateMachines = new ChildAnimatorStateMachine[stateMachineCount];

			// set destination state for transitions here because all states has become valid only now
			for (int i = 0; i < stateMachine.StateConstantArray.Length; i++)
			{
				AnimatorState state = states[i];
				StateConstant stateConstant = stateMachine.StateConstantArray[i].Instance;
				PPtr<AnimatorStateTransition>[] transitions = new PPtr<AnimatorStateTransition>[stateConstant.TransitionConstantArray.Length];
				for (int j = 0; j < stateConstant.TransitionConstantArray.Length; j++)
				{
					TransitionConstant transitionConstant = stateConstant.TransitionConstantArray[j].Instance;
					AnimatorStateTransition.Parameters parameters = new AnimatorStateTransition.Parameters
					{
						StateMachine = stateMachine,
						States = states,
						TOS = controller.TOS,
						Transition = transitionConstant,
						Version = controller.File.Version,
					};
					AnimatorStateTransition transition = AnimatorStateTransition.CreateVirtualInstance(virtualFile, parameters);
					transitions[j] = transition.File.CreatePPtr(transition);
				}
				state.Transitions = transitions;
			}

			AnyStateTransitions = new PPtr<AnimatorStateTransition>[stateMachine.AnyStateTransitionConstantArray.Length];
			for (int i = 0; i < stateMachine.AnyStateTransitionConstantArray.Length; i++)
			{
				TransitionConstant transitionConstant = stateMachine.AnyStateTransitionConstantArray[i].Instance;
				AnimatorStateTransition.Parameters parameters = new AnimatorStateTransition.Parameters
				{
					StateMachine = stateMachine,
					States = states,
					TOS = controller.TOS,
					Transition = transitionConstant,
					Version = controller.File.Version,
				};
				AnimatorStateTransition transition = AnimatorStateTransition.CreateVirtualInstance(virtualFile, parameters);
				AnyStateTransitions[i] = transition.File.CreatePPtr(transition);
			}

			StateMachineConstant.Parameters stateParameters = new StateMachineConstant.Parameters
			{
				ID = layer.Binding,
				States = states,
				TOS = controller.TOS,
				Version = controller.File.Version,
			};
			EntryTransitions = stateMachine.CreateEntryTransitions(virtualFile, stateParameters);
#warning TEMP: remove comment when AnimatorStateMachine's child StateMachines has been implemented
			//StateMachineBehaviours = controller.GetStateBehaviours(layerIndex);
			StateMachineBehaviours = Array.Empty<PPtr<MonoBehaviour>>();

			AnyStatePosition = new Vector3f(0.0f, -StateOffset, 0.0f);
			EntryPosition = new Vector3f(StateOffset, -StateOffset, 0.0f);
			ExitPosition = new Vector3f(2.0f * StateOffset, -StateOffset, 0.0f);
			ParentStateMachinePosition = new Vector3f(0.0f, -2.0f * StateOffset, 0.0f);

			DefaultState = ChildStates.Length > 0 ? ChildStates[stateMachine.DefaultState].State : default;
		}

		public static AnimatorStateMachine CreateVirtualInstance(VirtualSerializedFile virtualFile, AnimatorController controller, int stateMachineIndex)
		{
			return virtualFile.CreateAsset((assetInfo) => new AnimatorStateMachine(virtualFile.Layout, assetInfo, controller, stateMachineIndex));
		}

		public static int ToSerializedVersion(Version version)
		{
			// TODO:
			return 5;
		}

		public override void Read(AssetReader reader)
		{
			throw new NotSupportedException();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.InsertSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(ChildStatesName, ChildStates.ExportYAML(container));
			node.Add(ChildStateMachinesName, ChildStateMachines.ExportYAML(container));
			node.Add(AnyStateTransitionsName, AnyStateTransitions.ExportYAML(container));
			node.Add(EntryTransitionsName, EntryTransitions.ExportYAML(container));
			node.Add(StateMachineTransitionsName, StateMachineTransitions.ExportYAML(container));
			node.Add(StateMachineBehavioursName, StateMachineBehaviours.ExportYAML(container));
			node.Add(AnyStatePositionName, AnyStatePosition.ExportYAML(container));
			node.Add(EntryPositionName, EntryPosition.ExportYAML(container));
			node.Add(ExitPositionName, ExitPosition.ExportYAML(container));
			node.Add(ParentStateMachinePositionName, ParentStateMachinePosition.ExportYAML(container));
			node.Add(DefaultStateName, DefaultState.ExportYAML(container));
			return node;
		}

		public ChildAnimatorState[] ChildStates { get; set; }
		public ChildAnimatorStateMachine[] ChildStateMachines { get; set; }
		public PPtr<AnimatorStateTransition>[] AnyStateTransitions { get; set; }
		public PPtr<AnimatorTransition>[] EntryTransitions { get; set; }
		public Dictionary<PPtr<AnimatorStateMachine>, PPtr<AnimatorTransition>[]> StateMachineTransitions { get; set; } = new Dictionary<PPtr<AnimatorStateMachine>, PPtr<AnimatorTransition>[]>();
		public PPtr<MonoBehaviour>[] StateMachineBehaviours { get; set; }

		public Vector3f AnyStatePosition;
		public Vector3f EntryPosition;
		public Vector3f ExitPosition;
		public Vector3f ParentStateMachinePosition;
		public PPtr<AnimatorState> DefaultState;

		public const string ChildStatesName = "m_ChildStates";
		public const string ChildStateMachinesName = "m_ChildStateMachines";
		public const string AnyStateTransitionsName = "m_AnyStateTransitions";
		public const string EntryTransitionsName = "m_EntryTransitions";
		public const string StateMachineTransitionsName = "m_StateMachineTransitions";
		public const string StateMachineBehavioursName = "m_StateMachineBehaviours";
		public const string AnyStatePositionName = "m_AnyStatePosition";
		public const string EntryPositionName = "m_EntryPosition";
		public const string ExitPositionName = "m_ExitPosition";
		public const string ParentStateMachinePositionName = "m_ParentStateMachinePosition";
		public const string DefaultStateName = "m_DefaultState";

		private const float StateOffset = 250.0f;
	}
}
