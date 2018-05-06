using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class AnimatorStateMachine : NamedObject
	{
		public AnimatorStateMachine(AnimatorController controller) :
			base(CreateAssetsInfo(controller.File))
		{
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
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
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

		private readonly Dictionary<PPtr<AnimatorStateMachine>, PPtr<AnimatorTransition>[]> m_stateMachineTransitions = new Dictionary<PPtr<AnimatorStateMachine>, PPtr<AnimatorTransition>[]>();

		private ChildAnimatorState[] m_childStates;
		private ChildAnimatorStateMachine[] m_childStateMachines;
		private PPtr<AnimatorStateTransition>[] m_anyStateTransitions;
		private PPtr<AnimatorTransition>[] m_entryTransitions;
		private PPtr<MonoBehaviour>[] m_stateMachineBehaviours;
	}
}
