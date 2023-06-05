using AssetRipper.SourceGenerated.Subclasses.BlendTreeConstant;
using AssetRipper.SourceGenerated.Subclasses.StateConstant;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class StateConstantExtensions
	{
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
