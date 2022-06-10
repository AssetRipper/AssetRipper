using AssetRipper.Core.Classes.Misc;
using AssetRipper.SourceGenerated.Classes.ClassID_1102;
using AssetRipper.SourceGenerated.Classes.ClassID_207;
using AssetRipper.SourceGenerated.Subclasses.StateMotionPair;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class StateMotionPairExtensions
	{
		public static void SetValues(this IStateMotionPair pair, IAnimatorState state, IMotion motion)
		{
			if (state == null)
			{
				throw new ArgumentNullException(nameof(state));
			}
			if (motion == null)
			{
				throw new ArgumentNullException(nameof(motion));
			}
			pair.State.CopyValues(state.SerializedFile.CreatePPtr(state));
			pair.Motion.CopyValues(motion.SerializedFile.CreatePPtr(motion));
		}
	}
}
