using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class AnimatorState : NamedObject
	{
		private AnimatorState(AssetInfo assetInfo, AnimatorController controller, int stateMachineIndex, int stateIndex, Vector3f position) :
			base(assetInfo, 1)
		{
			VirtualSerializedFile virtualFile = (VirtualSerializedFile)assetInfo.File;
			
			IReadOnlyDictionary<uint, string> TOS = controller.TOS;
			StateMachineConstant stateMachine = controller.Controller.StateMachineArray[stateMachineIndex].Instance;
			StateConstant state = stateMachine.StateConstantArray[stateIndex].Instance;

			Name = TOS[state.NameID];

			Speed = state.Speed;
			CycleOffset = state.CycleOffset;
			
			m_transitions = new PPtr<AnimatorStateTransition>[state.TransitionConstantArray.Count];
			for(int i = 0; i < state.TransitionConstantArray.Count; i++)
			{
				TransitionConstant transitionConstant = state.TransitionConstantArray[i].Instance;
				AnimatorStateTransition transition = AnimatorStateTransition.CreateVirtualInstance(virtualFile, controller, transitionConstant);
				m_transitions[i] = transition.File.CreatePPtr(transition);
			}

			m_stateMachineBehaviours = controller.GetStateBeahviours(stateMachineIndex, stateIndex);
			Position = position;
			IKOnFeet = state.IKOnFeet;
			WriteDefaultValues = state.GetWriteDefaultValues(controller.File.Version);
			Mirror = state.Mirror;
			SpeedParameterActive = state.SpeedParamID > 0;
			MirrorParameterActive = state.MirrorParamID > 0;
			CycleOffsetParameterActive = state.CycleOffsetParamID > 0;
			TimeParameterActive = state.TimeParamID > 0;

			Motion = state.CreateMotion(virtualFile, controller, 0);

			Tag = TOS[state.TagID];
			SpeedParameter = TOS[state.SpeedParamID];
			MirrorParameter = TOS[state.MirrorParamID];
			CycleOffsetParameter = TOS[state.CycleOffsetParamID];
			TimeParameter = TOS[state.TimeParamID];
		}

		public static AnimatorState CreateVirtualInstance(VirtualSerializedFile virtualFile, AnimatorController controller, int stateMachineIndex,
			int stateIndex, Vector3f position)
		{
			return virtualFile.CreateAsset((assetInfo) => new AnimatorState(assetInfo, controller, stateMachineIndex, stateIndex, position));
		}

		private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 5;
		}
		
		public override void Read(AssetReader reader)
		{
			throw new NotSupportedException();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.InsertSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Speed", Speed);
			node.Add("m_CycleOffset", CycleOffset);
			node.Add("m_Transitions", Transitions.ExportYAML(container));
			node.Add("m_StateMachineBehaviours", StateMachineBehaviours.ExportYAML(container));
			node.Add("m_Position", Position.ExportYAML(container));
			node.Add("m_IKOnFeet", IKOnFeet);
			node.Add("m_WriteDefaultValues", WriteDefaultValues);
			node.Add("m_Mirror", Mirror);
			node.Add("m_SpeedParameterActive", SpeedParameterActive);
			node.Add("m_MirrorParameterActive", MirrorParameterActive);
			node.Add("m_CycleOffsetParameterActive", CycleOffsetParameterActive);
			node.Add("m_TimeParameterActive", TimeParameterActive);
			node.Add("m_Motion", Motion.ExportYAML(container));
			node.Add("m_Tag", Tag);
			node.Add("m_SpeedParameter", SpeedParameter);
			node.Add("m_MirrorParameter", MirrorParameter);
			node.Add("m_CycleOffsetParameter", CycleOffsetParameter);
			node.Add("m_TimeParameter", TimeParameter);
			return node;
		}

		public override string ExportExtension => throw new NotSupportedException();

		public float Speed { get; private set; }
		public float CycleOffset { get; private set; }
		public IReadOnlyList<PPtr<AnimatorStateTransition>> Transitions => m_transitions;
		public IReadOnlyList<PPtr<MonoBehaviour>> StateMachineBehaviours => m_stateMachineBehaviours;
		public bool IKOnFeet { get; private set; }
		public bool WriteDefaultValues { get; private set; }
		public bool Mirror { get; private set; }
		public bool SpeedParameterActive { get; private set; }
		public bool MirrorParameterActive { get; private set; }
		public bool CycleOffsetParameterActive { get; private set; }
		public bool TimeParameterActive { get; private set; }
		public string Tag { get; private set; }
		public string SpeedParameter { get; private set; }
		public string MirrorParameter { get; private set; }
		public string CycleOffsetParameter { get; private set; }
		public string TimeParameter { get; private set; }

		public Vector3f Position;
		public PPtr<Motion> Motion;

		private PPtr<AnimatorStateTransition>[] m_transitions;
		private PPtr<MonoBehaviour>[] m_stateMachineBehaviours;
	}
}
