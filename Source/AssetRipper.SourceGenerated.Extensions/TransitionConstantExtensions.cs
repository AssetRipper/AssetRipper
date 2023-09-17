using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.OffsetPtr_ConditionConstant;
using AssetRipper.SourceGenerated.Subclasses.TransitionConstant;

namespace AssetRipper.SourceGenerated.Extensions
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
				return transitionConstant.InterruptionSourceE;
			}
			else
			{
				return transitionConstant.Atomic ? TransitionInterruptionSource.None : TransitionInterruptionSource.Destination;
			}
		}

		public static bool IsExit(this ITransitionConstant transitionConstant) => transitionConstant.DestinationState >= 30000;
	}
}
