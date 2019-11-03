using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Converters;
using uTinyRipper.Classes.AnimatorControllers;

namespace uTinyRipper.Classes.BlendTrees
{
	public sealed class ChildMotion : IYAMLExportable
	{
		public ChildMotion(VirtualSerializedFile file, AnimatorController controller, StateConstant state, int nodeIndex, int childIndex)
		{
			BlendTreeConstant treeConstant = state.GetBlendTree();
			BlendTreeNodeConstant node = treeConstant.NodeArray[nodeIndex].Instance;
			int childNodeIndex = (int)node.ChildIndices[childIndex];
			Motion = state.CreateMotion(file, controller, childNodeIndex);

			Threshold = node.GetThreshold(controller.File.Version, childIndex);
			Position = default;
			TimeScale = 1.0f;
			CycleOffset = node.CycleOffset;

			uint directID = node.GetDirectBlendParameter(controller.File.Version, childIndex);
			DirectBlendParameter = controller.TOS[directID];

			Mirror = node.Mirror;
		}

		public static int ToSerializedVersion(Version version)
		{
			// TODO:
			return 2;
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(MotionName, Motion.ExportYAML(container));
			node.Add(ThresholdName, Threshold);
			node.Add(PositionName, Position.ExportYAML(container));
			node.Add(TimeScaleName, TimeScale);
			node.Add(CycleOffsetName, CycleOffset);
			node.Add(DirectBlendParameterName, DirectBlendParameter);
			node.Add(MirrorName, Mirror);
			return node;
		}

		public float Threshold { get; set; }
		public float TimeScale { get; set; }
		public float CycleOffset { get; set; }
		public string DirectBlendParameter { get; set; }
		public bool Mirror { get; set; }

		public const string MotionName = "m_Motion";
		public const string ThresholdName = "m_Threshold";
		public const string PositionName = "m_Position";
		public const string TimeScaleName = "m_TimeScale";
		public const string CycleOffsetName = "m_CycleOffset";
		public const string DirectBlendParameterName = "m_DirectBlendParameter";
		public const string MirrorName = "m_Mirror";

		public PPtr<Motion> Motion;
		public Vector2f Position;
	}
}
