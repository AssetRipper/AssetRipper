using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.ConditionConstant;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class ConditionConstantExtensions
	{
		public static AnimatorConditionMode GetConditionMode(this ConditionConstant constant)
		{
			return (AnimatorConditionMode)constant.ConditionMode;
		}
	}
}
