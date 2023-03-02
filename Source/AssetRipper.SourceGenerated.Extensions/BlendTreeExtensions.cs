using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_206;
using AssetRipper.SourceGenerated.Classes.ClassID_207;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.BlendTreeConstant;
using AssetRipper.SourceGenerated.Subclasses.BlendTreeNodeConstant;
using AssetRipper.SourceGenerated.Subclasses.ChildMotion;
using AssetRipper.SourceGenerated.Subclasses.StateConstant;
using BlendTreeType = AssetRipper.SourceGenerated.Enums.BlendTreeType_1;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class BlendTreeExtensions
	{
		public static BlendTreeType GetBlendType(this IBlendTree tree)
		{
			return tree.Has_BlendType_C206_Int32() ? (BlendTreeType)tree.BlendType_C206_Int32 : (BlendTreeType)tree.BlendType_C206_UInt32;
		}

		public static IChildMotion AddAndInitializeNewChild(this IBlendTree tree, ProcessedAssetCollection file, IAnimatorController controller, IStateConstant state, int nodeIndex, int childIndex)
		{
			IChildMotion childMotion = tree.Childs_C206.AddNew();
			IBlendTreeConstant treeConstant = state.GetBlendTree();
			IBlendTreeNodeConstant node = treeConstant.NodeArray[nodeIndex].Data;
			int childNodeIndex = (int)node.ChildIndices[childIndex];
			IMotion? motion = state.CreateMotion(file, controller, childNodeIndex);
			childMotion.Motion.CopyValues(tree.Collection.ForceCreatePPtr(motion));

			childMotion.Threshold = node.GetThreshold(childIndex);
			childMotion.Position?.CopyValues(node.GetPosition(childIndex));
			childMotion.TimeScale = 1.0f;
			childMotion.CycleOffset = node.CycleOffset;

			if(node.TryGetDirectBlendParameter(childIndex, out uint directID))
			{
				childMotion.DirectBlendParameter?.CopyValues(controller.TOS_C91[directID]);
			}

			childMotion.Mirror = node.Mirror;

			return childMotion;
		}
	}
}
