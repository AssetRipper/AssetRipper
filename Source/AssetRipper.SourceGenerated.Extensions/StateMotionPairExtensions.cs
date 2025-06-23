using AssetRipper.SourceGenerated.Classes.ClassID_1102;
using AssetRipper.SourceGenerated.Classes.ClassID_207;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Subclasses.StateMotionPair;

namespace AssetRipper.SourceGenerated.Extensions;

public static class StateMotionPairExtensions
{
	public static void SetValues(this IStateMotionPair pair, IAnimatorController controller, IAnimatorState state, IMotion motion)
	{
		if (state == null)
		{
			throw new ArgumentNullException(nameof(state));
		}
		if (motion == null)
		{
			throw new ArgumentNullException(nameof(motion));
		}
		pair.State.SetAsset(controller.Collection, state);
		pair.Motion.SetAsset(controller.Collection, motion);
	}
}
