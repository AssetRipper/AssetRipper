using AssetRipper.Core.Classes.AnimatorTransition;
using AssetRipper.SourceGenerated.Subclasses.AnimatorCondition;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class AnimatorConditionExtensions
	{
		public static AnimatorConditionMode GetConditionMode(this IAnimatorCondition condition)
		{
			return (AnimatorConditionMode)condition.ConditionMode;
		}
	}
}
