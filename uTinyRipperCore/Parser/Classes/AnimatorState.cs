using System;
using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Classes.Objects;
using uTinyRipper.Converters;
using uTinyRipper.Classes.AnimatorControllers;

namespace uTinyRipper.Classes
{
	public sealed class AnimatorState : NamedObject
	{
		private AnimatorState(AssetInfo assetInfo, AnimatorController controller, int stateMachineIndex, int stateIndex, Vector3f position) :
			base(assetInfo, HideFlags.HideInHierarchy)
		{
			VirtualSerializedFile virtualFile = (VirtualSerializedFile)assetInfo.File;

			IReadOnlyDictionary<uint, string> TOS = controller.TOS;
			if (!TOS.ContainsKey(0))
			{
				Dictionary<uint, string> tos = new Dictionary<uint, string>();
				tos.Add(0, string.Empty);
				tos.AddRange(controller.TOS);
				TOS = tos;
			}
			StateMachineConstant stateMachine = controller.Controller.StateMachineArray[stateMachineIndex].Instance;
			StateConstant state = stateMachine.StateConstantArray[stateIndex].Instance;

			Name = TOS[state.NameID];

			Speed = state.Speed;
			CycleOffset = state.CycleOffset;

			// skip Transitions because not all state exists right now

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
			node.InsertSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(SpeedName, Speed);
			node.Add(CycleOffsetName, CycleOffset);
			node.Add(TransitionsName, Transitions.ExportYAML(container));
			node.Add(StateMachineBehavioursName, StateMachineBehaviours.ExportYAML(container));
			node.Add(PositionName, Position.ExportYAML(container));
			node.Add(IKOnFeetName, IKOnFeet);
			node.Add(WriteDefaultValuesName, WriteDefaultValues);
			node.Add(MirrorName, Mirror);
			node.Add(SpeedParameterActiveName, SpeedParameterActive);
			node.Add(MirrorParameterActiveName, MirrorParameterActive);
			node.Add(CycleOffsetParameterActiveName, CycleOffsetParameterActive);
			node.Add(TimeParameterActiveName, TimeParameterActive);
			node.Add(MotionName, Motion.ExportYAML(container));
			node.Add(TagName, Tag);
			node.Add(SpeedParameterName, SpeedParameter);
			node.Add(MirrorParameterName, MirrorParameter);
			node.Add(CycleOffsetParameterName, CycleOffsetParameter);
			node.Add(TimeParameterName, TimeParameter);
			return node;
		}

		public override string ExportExtension => throw new NotSupportedException();

		public float Speed { get; private set; }
		public float CycleOffset { get; private set; }
		public IReadOnlyList<PPtr<AnimatorStateTransition>> Transitions { get; set; }
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

		public const string SpeedName = "m_Speed";
		public const string CycleOffsetName = "m_CycleOffset";
		public const string TransitionsName = "m_Transitions";
		public const string StateMachineBehavioursName = "m_StateMachineBehaviours";
		public const string PositionName = "m_Position";
		public const string IKOnFeetName = "m_IKOnFeet";
		public const string WriteDefaultValuesName = "m_WriteDefaultValues";
		public const string MirrorName = "m_Mirror";
		public const string SpeedParameterActiveName = "m_SpeedParameterActive";
		public const string MirrorParameterActiveName = "m_MirrorParameterActive";
		public const string CycleOffsetParameterActiveName = "m_CycleOffsetParameterActive";
		public const string TimeParameterActiveName = "m_TimeParameterActive";
		public const string MotionName = "m_Motion";
		public const string TagName = "m_Tag";
		public const string SpeedParameterName = "m_SpeedParameter";
		public const string MirrorParameterName = "m_MirrorParameter";
		public const string CycleOffsetParameterName = "m_CycleOffsetParameter";
		public const string TimeParameterName = "m_TimeParameter";

		public Vector3f Position;
		public PPtr<Motion> Motion;

		private PPtr<MonoBehaviour>[] m_stateMachineBehaviours;
	}
}
