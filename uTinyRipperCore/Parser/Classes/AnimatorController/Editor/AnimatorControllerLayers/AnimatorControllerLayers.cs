using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class AnimatorControllerLayers : IYAMLExportable
	{
		public AnimatorControllerLayers(AnimatorStateMachine stateMachine, AnimatorController controller, int layerIndex)
		{
			LayerConstant layer = controller.Controller.LayerArray[layerIndex].Instance;

			stateMachine.ParentStateMachinePosition = new Vector3f(800.0f, 20.0f, 0.0f);

			Name = controller.TOS[layer.Binding];

			StateMachine = stateMachine.File.CreatePPtr(stateMachine);

#warning TODO: animator
			Mask = default;

			m_motions = new StateMotionPair[0];
			m_behaviours = new StateBehavioursPair[0];
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
			node.Add("m_Name", Name);
			node.Add("m_StateMachine", StateMachine.ExportYAML(container));
			node.Add("m_Mask", Mask.ExportYAML(container));
			node.Add("m_Motions", Motions.ExportYAML(container));
			node.Add("m_Behaviours", Behaviours.ExportYAML(container));
			node.Add("m_BlendingMode", (int)BlendingMode);
			node.Add("m_SyncedLayerIndex", SyncedLayerIndex);
			node.Add("m_DefaultWeight", DefaultWeight);
			node.Add("m_IKPass", IKPass);
			node.Add("m_SyncedLayerAffectsTiming", SyncedLayerAffectsTiming);
			node.Add("m_Controller", Controller.ExportYAML(container));
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

		private StateMotionPair[] m_motions;
		private StateBehavioursPair[] m_behaviours;
	}
}
