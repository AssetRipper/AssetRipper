using System.Collections.Generic;
using uTinyRipper.Project;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper.Classes;

namespace uTinyRipper.Classes.AnimatorControllers
{
	public sealed class AnimatorControllerLayer : IYAMLExportable
	{
		public AnimatorControllerLayer(AnimatorStateMachine stateMachine, AnimatorController controller, int layerIndex)
		{
			LayerConstant layer = controller.Controller.LayerArray[layerIndex].Instance;

			stateMachine.ParentStateMachinePosition = new Vector3f(800.0f, 20.0f, 0.0f);

			Name = controller.TOS[layer.Binding];

			StateMachine = stateMachine.File.CreatePPtr(stateMachine);

#warning TODO: animator
			Mask = default;

			m_motions = System.Array.Empty<StateMotionPair>();
			m_behaviours = System.Array.Empty<StateBehavioursPair>();
			BlendingMode = layer.LayerBlendingMode;
			SyncedLayerIndex = layer.StateMachineMotionSetIndex == 0 ? -1 : layer.StateMachineIndex;
			DefaultWeight = layer.DefaultWeight;
			IKPass = layer.IKPass;
			SyncedLayerAffectsTiming = layer.SyncedLayerAffectsTiming;
			Controller = controller.File.CreatePPtr(controller);
		}

		private static int GetSerializedVersion(Version version)
		{
			// TODO:
			return 5;
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(NameName, Name);
			node.Add(StateMachineName, StateMachine.ExportYAML(container));
			node.Add(MaskName, Mask.ExportYAML(container));
			node.Add(MotionsName, Motions.ExportYAML(container));
			node.Add(BehavioursName, Behaviours.ExportYAML(container));
			node.Add(BlendingModeName, (int)BlendingMode);
			node.Add(SyncedLayerIndexName, SyncedLayerIndex);
			node.Add(DefaultWeightName, DefaultWeight);
			node.Add(IKPassName, IKPass);
			node.Add(SyncedLayerAffectsTimingName, SyncedLayerAffectsTiming);
			node.Add(ControllerName, Controller.ExportYAML(container));
			return node;
		}

		public string Name { get; private set; }
		public IReadOnlyList<StateMotionPair> Motions => m_motions;
		public IReadOnlyList<StateBehavioursPair> Behaviours => m_behaviours;
		public AnimatorLayerBlendingMode BlendingMode { get; private set; }
		public int SyncedLayerIndex { get; private set; }
		public float DefaultWeight { get; private set; }
		public bool IKPass { get; private set; }
		public bool SyncedLayerAffectsTiming { get; private set; }

		public PPtr<AnimatorStateMachine> StateMachine;
		public PPtr<AvatarMask> Mask;
		public PPtr<AnimatorController> Controller;

		public const string NameName = "m_Name";
		public const string StateMachineName = "m_StateMachine";
		public const string MaskName = "m_Mask";
		public const string MotionsName = "m_Motions";
		public const string BehavioursName = "m_Behaviours";
		public const string BlendingModeName = "m_BlendingMode";
		public const string SyncedLayerIndexName = "m_SyncedLayerIndex";
		public const string DefaultWeightName = "m_DefaultWeight";
		public const string IKPassName = "m_IKPass";
		public const string SyncedLayerAffectsTimingName = "m_SyncedLayerAffectsTiming";
		public const string ControllerName = "m_Controller";

		private StateMotionPair[] m_motions;
		private StateBehavioursPair[] m_behaviours;
	}
}
