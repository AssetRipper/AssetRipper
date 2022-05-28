using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.SourceGenerated.Classes.ClassID_206;
using AssetRipper.SourceGenerated.Classes.ClassID_207;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Subclasses.BlendTreeConstant;
using AssetRipper.SourceGenerated.Subclasses.BlendTreeNodeConstant;
using AssetRipper.SourceGenerated.Subclasses.LeafInfoConstant;
using AssetRipper.SourceGenerated.Subclasses.StateConstant;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class StateConstantExtensions
	{
		public static PPtr<Motion> CreateMotion(this IStateConstant stateConstant, VirtualSerializedFile file, IAnimatorController controller, int nodeIndex)
		{
			if (stateConstant.BlendTreeConstantArray.Count == 0)
			{
				return new();
			}
			else
			{
				IBlendTreeNodeConstant node = stateConstant.GetBlendTree().NodeArray[nodeIndex].Data;
				if (node.IsBlendTree())
				{
					IBlendTree blendTree = VirtualAnimationFactory.CreateBlendTree(file, controller, stateConstant, nodeIndex);
					return blendTree.SerializedFile.CreatePPtr(blendTree).CastTo<Motion>();
				}
				else
				{
					int clipIndex = -1;
					if (stateConstant.Has_LeafInfoArray())
					{
						for (int i = 0; i < stateConstant.LeafInfoArray.Count; i++)
						{
							LeafInfoConstant leafInfo = stateConstant.LeafInfoArray[i];
							int index = leafInfo.m_IDArray.IndexOf(node.ClipID);
							if (index >= 0)
							{
								clipIndex = (int)leafInfo.m_IndexOffset + index;
								break;
							}
						}
					}
					else
					{
						clipIndex = unchecked((int)node.ClipID);
					}
					return node.CreateMotion(controller, clipIndex).CastTo<Motion>();
				}
			}
		}

		public static bool IsBlendTree(this IStateConstant stateConstant)
		{
			if (stateConstant.BlendTreeConstantArray.Count == 0)
			{
				return false;
			}
			return stateConstant.GetBlendTree().NodeArray.Count > 1;
		}

		public static IBlendTreeConstant GetBlendTree(this IStateConstant stateConstant)
		{
			return stateConstant.BlendTreeConstantArray[0].Data;
		}

		public static bool GetWriteDefaultValues(this IStateConstant stateConstant)
		{
			return !stateConstant.Has_WriteDefaultValues() || stateConstant.WriteDefaultValues;
		}
	}
}
