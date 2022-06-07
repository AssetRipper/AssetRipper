using AssetRipper.Core.Classes.Misc.KeyframeTpl;
using AssetRipper.Core.Classes.Misc.KeyframeTpl.TangentMode;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.SourceGenerated.Subclasses.Keyframe_Vector3f;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static partial class KeyframeExtensions
	{
		public static void SetValues(this IKeyframe_Vector3f keyframe, UnityVersion version, float time, IVector3f value, IVector3f weight)
		{
			keyframe.Time = time;
			keyframe.Value.CopyValuesFrom(value);
			// this enum member is version agnostic
			keyframe.TangentMode = TangentMode.FreeSmooth.ToTangent(version);
			keyframe.WeightedMode = (int)WeightedMode.None;
			keyframe.InWeight?.CopyValuesFrom(weight);
			keyframe.OutWeight?.CopyValuesFrom(weight);
		}

		public static void SetValues(this IKeyframe_Vector3f keyframe, UnityVersion version, float time, IVector3f value, IVector3f inSlope, IVector3f outSlope, IVector3f weight)
		{
			keyframe.Time = time;
			keyframe.Value.CopyValuesFrom(value);
			keyframe.InSlope.CopyValuesFrom(inSlope);
			keyframe.OutSlope.CopyValuesFrom(outSlope);
			// this enum member is version agnostic
			keyframe.TangentMode = TangentMode.FreeFree.ToTangent(version);
			keyframe.WeightedMode = (int)WeightedMode.None;
			keyframe.InWeight?.CopyValuesFrom(weight);
			keyframe.OutWeight?.CopyValuesFrom(weight);
		}
	}
}
