using AssetRipper.SourceGenerated.Subclasses.BlendTreeNodeConstant;
using AssetRipper.SourceGenerated.Subclasses.Vector2f;
using BlendTreeType = AssetRipper.SourceGenerated.Enums.BlendTreeType_1;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class BlendTreeNodeConstantExtensions
	{
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

		public static Vector2f_3_5_0 GetPosition(this IBlendTreeNodeConstant constant, int index)
		{
			if (constant.Has_Blend2dData() && constant.GetBlendType() != BlendTreeType.Simple1D && constant.GetBlendType() != BlendTreeType.Direct)
			{
				return constant.Blend2dData.Data.ChildPositionArray[index];
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

		public static bool TryGetDirectBlendParameter(this IBlendTreeNodeConstant constant, int index, out uint parameter)
		{
			if (constant.Has_BlendDirectData() && constant.GetBlendType() == BlendTreeType.Direct)
			{
				parameter = constant.BlendDirectData.Data.ChildBlendEventIDArray[index];
				return true;
			}
			parameter = default;
			return false;
		}

		public static bool IsBlendTree(this IBlendTreeNodeConstant constant) => constant.ChildIndices.Length > 0;
		public static BlendTreeType GetBlendType(this IBlendTreeNodeConstant constant) => (BlendTreeType)constant.BlendType;
	}
}
