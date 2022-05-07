using AssetRipper.Core.Classes.AnimatorController.Constants;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System;
using System.Collections.Generic;


namespace AssetRipper.Core.Classes
{
	public sealed class AnimatorState : NamedObject
	{
		private AnimatorState(LayoutInfo layout, AssetInfo assetInfo, AnimatorController.AnimatorController controller, int stateMachineIndex, int stateIndex, Vector3f position) : base(layout)
		{
			AssetInfo = assetInfo;
			ObjectHideFlagsOld = HideFlags.HideInHierarchy;

			VirtualSerializedFile virtualFile = (VirtualSerializedFile)assetInfo.File;

			IReadOnlyDictionary<uint, string> TOS = controller.TOS;
			if (!TOS.ContainsKey(0))
			{
				Dictionary<uint, string> tos = new Dictionary<uint, string>() { { 0, string.Empty } };
				tos.AddRange(controller.TOS);
				TOS = tos;
			}
			StateMachineConstant stateMachine = controller.Controller.StateMachineArray[stateMachineIndex].Instance;
			StateConstant state = stateMachine.StateConstantArray[stateIndex].Instance;

			NameString = TOS[state.NameID];

			Speed = state.Speed;
			CycleOffset = state.CycleOffset;

			// skip Transitions because not all state exists at this moment

			// exclude StateMachine's behaviours
			int layerIndex = controller.Controller.GetLayerIndexByStateMachineIndex(stateMachineIndex);
			PPtr<MonoBehaviour>[] machineBehaviours = controller.GetStateBehaviours(layerIndex);
			PPtr<MonoBehaviour>[] stateBehaviours = controller.GetStateBehaviours(stateMachineIndex, stateIndex);
			List<PPtr<MonoBehaviour>> behaviours = new List<PPtr<MonoBehaviour>>(stateBehaviours.Length);
			foreach (PPtr<MonoBehaviour> ptr in stateBehaviours)
			{
#warning TEMP: remove comment when AnimatorStateMachine's child StateMachines has been implemented
				//if (!machineBehaviours.Contains(ptr))
				{
					behaviours.Add(ptr);
				}
			}
			StateMachineBehaviours = behaviours.ToArray();

			Position = position;
			IKOnFeet = state.IKOnFeet;
			WriteDefaultValues = state.GetWriteDefaultValues(controller.SerializedFile.Version);
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

		public static AnimatorState CreateVirtualInstance(VirtualSerializedFile virtualFile, AnimatorController.AnimatorController controller, int stateMachineIndex,
			int stateIndex, Vector3f position)
		{
			return virtualFile.CreateAsset((assetInfo) => new AnimatorState(virtualFile.Layout, assetInfo, controller, stateMachineIndex, stateIndex, position));
		}

		public static int ToSerializedVersion(UnityVersion version)
		{
#warning TODO:
			return 5;
		}

		public override void Read(AssetReader reader)
		{
			throw new NotSupportedException();
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.InsertSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(SpeedName, Speed);
			node.Add(CycleOffsetName, CycleOffset);
			node.Add(TransitionsName, Transitions.ExportYaml(container));
			node.Add(StateMachineBehavioursName, StateMachineBehaviours.ExportYaml(container));
			node.Add(PositionName, Position.ExportYaml(container));
			node.Add(IKOnFeetName, IKOnFeet);
			node.Add(WriteDefaultValuesName, WriteDefaultValues);
			node.Add(MirrorName, Mirror);
			node.Add(SpeedParameterActiveName, SpeedParameterActive);
			node.Add(MirrorParameterActiveName, MirrorParameterActive);
			node.Add(CycleOffsetParameterActiveName, CycleOffsetParameterActive);
			node.Add(TimeParameterActiveName, TimeParameterActive);
			node.Add(MotionName, Motion.ExportYaml(container));
			node.Add(TagName, Tag);
			node.Add(SpeedParameterName, SpeedParameter);
			node.Add(MirrorParameterName, MirrorParameter);
			node.Add(CycleOffsetParameterName, CycleOffsetParameter);
			node.Add(TimeParameterName, TimeParameter);
			return node;
		}

		public override string ExportExtension => throw new NotSupportedException();

		public float Speed { get; set; }
		public float CycleOffset { get; set; }
		public PPtr<AnimatorStateTransition>[] Transitions { get; set; }
		public PPtr<MonoBehaviour>[] StateMachineBehaviours { get; set; }
		public bool IKOnFeet { get; set; }
		public bool WriteDefaultValues { get; set; }
		public bool Mirror { get; set; }
		public bool SpeedParameterActive { get; set; }
		public bool MirrorParameterActive { get; set; }
		public bool CycleOffsetParameterActive { get; set; }
		public bool TimeParameterActive { get; set; }
		public string Tag { get; set; }
		public string SpeedParameter { get; set; }
		public string MirrorParameter { get; set; }
		public string CycleOffsetParameter { get; set; }
		public string TimeParameter { get; set; }

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

		public Vector3f Position = new();
		public PPtr<Motion> Motion = new();
	}
}
