using AssetRipper.Core.Classes.AnimatorController.Constants;
using AssetRipper.Core.Classes.AnimatorTransition;
using AssetRipper.SourceGenerated.Subclasses.OffsetPtr_ConditionConstant;
using AssetRipper.SourceGenerated.Subclasses.TransitionConstant;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class TransitionConstantExtensions
	{
		public static bool GetHasFixedDuration(this ITransitionConstant transitionConstant)
		{
			return !transitionConstant.Has_HasFixedDuration() || transitionConstant.HasFixedDuration;
		}

		public static TransitionInterruptionSource GetInterruptionSource(this ITransitionConstant transitionConstant)
		{
			if (transitionConstant.Has_InterruptionSource())
			{
				return (TransitionInterruptionSource)transitionConstant.InterruptionSource;
			}
			else
			{
				return transitionConstant.Atomic ? TransitionInterruptionSource.None : TransitionInterruptionSource.Destination;
			}
		}

		public static float GetExitTime(this ITransitionConstant transitionConstant)
		{
			if (transitionConstant.Has_ExitTime())
			{
				return transitionConstant.ExitTime;
			}
			else
			{
				foreach (OffsetPtr_ConditionConstant conditionPtr in transitionConstant.ConditionConstantArray)
				{
					if (conditionPtr.Data.ConditionMode == (int)AnimatorConditionMode.ExitTime)
					{
						return conditionPtr.Data.ExitTime;
					}
				}
				return 1.0f;
			}
		}

		public static bool GetHasExitTime(this ITransitionConstant transitionConstant)
		{
			if (transitionConstant.Has_HasExitTime())
			{
				return transitionConstant.HasExitTime;
			}
			else
			{
				foreach (OffsetPtr_ConditionConstant conditionPtr in transitionConstant.ConditionConstantArray)
				{
					if (conditionPtr.Data.ConditionMode == (int)AnimatorConditionMode.ExitTime)
					{
						return true;
					}
				}
				return false;
			}
		}

		public static bool IsExit(this ITransitionConstant transitionConstant) => transitionConstant.DestinationState >= 30000;
	}
}
