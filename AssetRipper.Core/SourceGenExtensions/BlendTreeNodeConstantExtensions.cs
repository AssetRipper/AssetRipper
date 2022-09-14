using AssetRipper.Core.Classes.BlendTree;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Subclasses.BlendTreeNodeConstant;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimationClip_;
using AssetRipper.SourceGenerated.Subclasses.Vector2f;
using System.Linq;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class BlendTreeNodeConstantExtensions
	{
		public static IPPtr_AnimationClip_ CreateMotion(this IBlendTreeNodeConstant constant, IAnimatorController controller, int clipIndex)
		{
			if (clipIndex == -1)
			{
				return PPtr_AnimationClip_Factory.CreateAsset(controller.SerializedFile.Version);
			}
			else
			{
				return controller.AnimationClips_C91[clipIndex];
			}
		}

		public static float GetThreshold(this IBlendTreeNodeConstant constant, int index)
		{
			if (constant.Has_Blend1dData())
			{
				if (constant.GetBlendType() == BlendTreeType.Simple1D)
				{
					return constant.Blend1dData.Data.ChildThresholdArray[index];
				}
			}
			else if (constant.Has_ChildThresholdArray())
			{
				return constant.ChildThresholdArray[index];
			}
			return 0.0f;
		}

		public static Vector2f_3_5_0_f5 GetPosition(this IBlendTreeNodeConstant constant, int index)
		{
			if (constant.Has_Blend2dData() && constant.GetBlendType() != BlendTreeType.Simple1D && constant.GetBlendType() != BlendTreeType.Direct)
			{
				return constant.Blend2dData.Data.m_ChildPositionArray[index];
			}
			return new();
		}

		public static float GetMinThreshold(this IBlendTreeNodeConstant constant)
		{
			if (constant.Has_Blend1dData() && constant.GetBlendType() == BlendTreeType.Simple1D)
			{
				return constant.Blend1dData.Data.ChildThresholdArray.Min();
			}
			return 0.0f;
		}

		public static float GetMaxThreshold(this IBlendTreeNodeConstant constant)
		{
			if (constant.Has_Blend1dData() && constant.GetBlendType() == BlendTreeType.Simple1D)
			{
				return constant.Blend1dData.Data.ChildThresholdArray.Max();
			}
			return 1.0f;
		}

		public static uint GetDirectBlendParameter(this IBlendTreeNodeConstant constant, int index)
		{
			if (constant.Has_BlendDirectData() && constant.GetBlendType() == BlendTreeType.Direct)
			{
				return constant.BlendDirectData.Data.m_ChildBlendEventIDArray[index];
			}
			return 0;
		}

		public static bool IsBlendTree(this IBlendTreeNodeConstant constant) => constant.ChildIndices.Length > 0;
		public static BlendTreeType GetBlendType(this IBlendTreeNodeConstant constant) => (BlendTreeType)constant.BlendType;
	}
}
