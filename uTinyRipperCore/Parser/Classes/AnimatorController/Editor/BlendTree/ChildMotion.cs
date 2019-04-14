using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.AnimatorControllers.Editor
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

		private static int GetSerializedVersion(Version version)
		{
			// TODO:
			return 2;
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add("m_Motion", Motion.ExportYAML(container));
			node.Add("m_Threshold", Threshold);
			node.Add("m_Position", Position.ExportYAML(container));
			node.Add("m_TimeScale", TimeScale);
			node.Add("m_CycleOffset", CycleOffset);
			node.Add("m_DirectBlendParameter", DirectBlendParameter);
			node.Add("m_Mirror", Mirror);
			return node;
		}

		public float Threshold { get; private set; }
		public float TimeScale { get; private set; }
		public float CycleOffset { get; private set; }
		public string DirectBlendParameter { get; private set; }
		public bool Mirror { get; private set; }

		public PPtr<Motion> Motion;
		public Vector2f Position;
	}
}
