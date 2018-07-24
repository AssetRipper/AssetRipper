using System;
using System.Collections.Generic;
using System.Linq;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class AnimatorStateMachine : NamedObject
	{
		public AnimatorStateMachine(VirtualSerializedFile file, AnimatorController controller, int stateMachineIndex) :
			base(file.CreateAssetInfo(ClassIDType.AnimatorStateMachine))
		{
			ObjectHideFlags = 1;

			LayerConstant layer = controller.Controller.GetLayerByStateMachineIndex(stateMachineIndex);
			Name = controller.TOS[layer.Binding];

			StateMachineConstant stateMachine = controller.Controller.StateMachineArray[stateMachineIndex].Instance;
			
			int stateCount = stateMachine.StateConstantArray.Count;
			int stateMachineCount = 0;
			int count = stateCount + stateMachineCount;
			int side = (int)Math.Ceiling(Math.Sqrt(count));

			List<AnimatorState> states = new List<AnimatorState>();
			m_childStates = new ChildAnimatorState[stateCount];
			for (int y = 0, stateIndex = 0; y < side && stateIndex < stateCount; y++)
			{
				for (int x = 0; x < side && stateIndex < stateCount; x++, stateIndex++)
				{
					Vector3f position = new Vector3f(x * StateOffset, y * StateOffset, 0.0f);
					AnimatorState state = new AnimatorState(file, controller, stateMachineIndex, stateIndex, position);
					ChildAnimatorState childState = new ChildAnimatorState(state, position);
					m_childStates[stateIndex] = childState;
					states.Add(state);
				}
			}
			m_childStateMachines = new ChildAnimatorStateMachine[stateMachineCount];

			// set destination state for transitions here because all states has become valid only now
			for (int i = 0; i < stateMachine.StateConstantArray.Count; i++)
			{
				AnimatorState state = states[i];
				StateConstant stateConstant = stateMachine.StateConstantArray[i].Instance;
				for(int j = 0; j < stateConstant.TransitionConstantArray.Count; j++)
				{
					long stateTransitionPath = state.Transitions[j].PathID;
					AnimatorStateTransition transition = (AnimatorStateTransition)file.GetObject(stateTransitionPath);
					TransitionConstant transitionConstant = stateConstant.TransitionConstantArray[j].Instance;
					if (!transitionConstant.IsExit)
					{
						AnimatorState destState = states[transitionConstant.DestinationState];
						transition.DstState = PPtr<AnimatorState>.CreateVirtualPointer(destState);
					}
				}
			}

			m_anyStateTransitions = new PPtr<AnimatorStateTransition>[stateMachine.AnyStateTransitionConstantArray.Count];
			for(int i = 0; i < stateMachine.AnyStateTransitionConstantArray.Count; i++)
			{
				TransitionConstant transitionConstant = stateMachine.AnyStateTransitionConstantArray[i].Instance;
				AnimatorStateTransition transition = new AnimatorStateTransition(file, controller, transitionConstant, states);
				m_anyStateTransitions[i] = PPtr<AnimatorStateTransition>.CreateVirtualPointer(transition);
			}

			m_entryTransitions = stateMachine.GetEntryTransitions(file, controller, layer.Binding, states);
			m_stateMachineBehaviours = new PPtr<MonoBehaviour>[0];
			
			AnyStatePosition = new Vector3f(0.0f, -StateOffset, 0.0f);
			EntryPosition = new Vector3f(StateOffset, -StateOffset, 0.0f);
			ExitPosition = new Vector3f(2.0f * StateOffset, -StateOffset, 0.0f);
			ParentStateMachinePosition = new Vector3f(0.0f, -2.0f * StateOffset, 0.0f);

			DefaultState = ChildStates.Count > 0 ? ChildStates[stateMachine.DefaultState].State : default;

			file.AddAsset(this);
		}

		private static AssetInfo CreateAssetsInfo(ISerializedFile file)
		{
			return new AssetInfo(file, 0, ClassIDType.AnimatorStateMachine);
		}

		private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 5;
		}

		public override void Read(AssetStream stream)
		{
			throw new NotSupportedException();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.InsertSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_ChildStates", ChildStates.ExportYAML(container));
			node.Add("m_ChildStateMachines", ChildStateMachines.ExportYAML(container));
			node.Add("m_AnyStateTransitions", AnyStateTransitions.ExportYAML(container));
			node.Add("m_EntryTransitions", EntryTransitions.ExportYAML(container));
			node.Add("m_StateMachineTransitions", StateMachineTransitions.ExportYAMLArrayPPtr(container));
			node.Add("m_StateMachineBehaviours", StateMachineBehaviours.ExportYAML(container));
			node.Add("m_AnyStatePosition", AnyStatePosition.ExportYAML(container));
			node.Add("m_EntryPosition", EntryPosition.ExportYAML(container));
			node.Add("m_ExitPosition", ExitPosition.ExportYAML(container));
			node.Add("m_ParentStateMachinePosition", ParentStateMachinePosition.ExportYAML(container));
			node.Add("m_DefaultState", DefaultState.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<ChildAnimatorState> ChildStates => m_childStates;
		public IReadOnlyList<ChildAnimatorStateMachine> ChildStateMachines => m_childStateMachines;
		public IReadOnlyList<PPtr<AnimatorStateTransition>> AnyStateTransitions => m_anyStateTransitions;
		public IReadOnlyList<PPtr<AnimatorTransition>> EntryTransitions => m_entryTransitions;
		public IReadOnlyDictionary<PPtr<AnimatorStateMachine>, PPtr<AnimatorTransition>[]> StateMachineTransitions => m_stateMachineTransitions;
		public IReadOnlyList<PPtr<MonoBehaviour>> StateMachineBehaviours => m_stateMachineBehaviours;
		
		public Vector3f AnyStatePosition;
		public Vector3f EntryPosition;
		public Vector3f ExitPosition;
		public Vector3f ParentStateMachinePosition;
		public PPtr<AnimatorState> DefaultState;

		private const float StateOffset = 250.0f; 

		private readonly Dictionary<PPtr<AnimatorStateMachine>, PPtr<AnimatorTransition>[]> m_stateMachineTransitions = new Dictionary<PPtr<AnimatorStateMachine>, PPtr<AnimatorTransition>[]>();

		private ChildAnimatorState[] m_childStates;
		private ChildAnimatorStateMachine[] m_childStateMachines;
		private PPtr<AnimatorStateTransition>[] m_anyStateTransitions;
		private PPtr<AnimatorTransition>[] m_entryTransitions;
		private PPtr<MonoBehaviour>[] m_stateMachineBehaviours;
	}
}
