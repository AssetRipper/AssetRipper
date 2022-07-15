using AssetRipper.Core.Classes.AnimatorTransition;
using AssetRipper.SourceGenerated.Subclasses.ConditionConstant;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class ConditionConstantExtensions
	{
		public static AnimatorConditionMode GetConditionMode(this ConditionConstant constant)
		{
			return (AnimatorConditionMode)constant.ConditionMode;
		}
	}
}
