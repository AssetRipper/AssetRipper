using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Subclasses.BlendTreeConstant;
using AssetRipper.SourceGenerated.Subclasses.BlendTreeNodeConstant;
using AssetRipper.SourceGenerated.Subclasses.ChildMotion;
using AssetRipper.SourceGenerated.Subclasses.StateConstant;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class ChildMotionExtensions
	{
		public static void SetValues(this IChildMotion childMotion, VirtualSerializedFile file, IAnimatorController controller, IStateConstant state, int nodeIndex, int childIndex)
		{
			IBlendTreeConstant treeConstant = state.GetBlendTree();
			IBlendTreeNodeConstant node = treeConstant.NodeArray[nodeIndex].Data;
			int childNodeIndex = (int)node.ChildIndices[childIndex];
			childMotion.Motion.CopyValues(state.CreateMotion(file, controller, childNodeIndex));

			childMotion.Threshold = node.GetThreshold(childIndex);
			childMotion.Position?.CopyValues(node.GetPosition(childIndex));
			childMotion.TimeScale = 1.0f;
			childMotion.CycleOffset = node.CycleOffset;

			uint directID = node.GetDirectBlendParameter(childIndex);
			childMotion.DirectBlendParameter?.CopyValues(controller.TOS_C91[directID]);

			childMotion.Mirror = node.Mirror;
		}
	}
}
