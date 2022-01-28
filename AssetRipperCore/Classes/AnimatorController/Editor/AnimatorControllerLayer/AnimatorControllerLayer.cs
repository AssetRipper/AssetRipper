using AssetRipper.Core.Classes.AnimatorController.Constants;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.AnimatorController.Editor.AnimatorControllerLayer
{
	public sealed class AnimatorControllerLayer : IYAMLExportable
	{
		public AnimatorControllerLayer(AnimatorStateMachine.AnimatorStateMachine stateMachine, AnimatorController controller, int layerIndex)
		{
			LayerConstant layer = controller.Controller.LayerArray[layerIndex].Instance;

			stateMachine.ParentStateMachinePosition = new Vector3f(800.0f, 20.0f, 0.0f);

			Name = controller.TOS[layer.Binding];

			StateMachine = stateMachine.SerializedFile.CreatePPtr(stateMachine);

#warning TODO: animator
			Mask = new();

			Motions = System.Array.Empty<StateMotionPair>();
			Behaviours = System.Array.Empty<StateBehavioursPair>();
			BlendingMode = layer.LayerBlendingMode;
			SyncedLayerIndex = layer.StateMachineMotionSetIndex == 0 ? -1 : layer.StateMachineIndex;
			DefaultWeight = layer.DefaultWeight;
			IKPass = layer.IKPass;
			SyncedLayerAffectsTiming = layer.SyncedLayerAffectsTiming;
			Controller = controller.SerializedFile.CreatePPtr(controller);
		}

		public static int ToSerializedVersion(UnityVersion version)
		{
#warning TODO:
			return 5;
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
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

		public string Name { get; set; }
		public StateMotionPair[] Motions { get; set; }
		public StateBehavioursPair[] Behaviours { get; set; }
		public AnimatorLayerBlendingMode BlendingMode { get; set; }
		public int SyncedLayerIndex { get; set; }
		public float DefaultWeight { get; set; }
		public bool IKPass { get; set; }
		public bool SyncedLayerAffectsTiming { get; set; }

		public PPtr<AnimatorStateMachine.AnimatorStateMachine> StateMachine = new();
		public PPtr<AvatarMask.AvatarMask> Mask = new();
		public PPtr<AnimatorController> Controller = new();

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
	}
}
