using AssetRipper.Core.Classes.Misc.KeyframeTpl;
using AssetRipper.Core.Classes.Misc.KeyframeTpl.TangentMode;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.SourceGenerated.Subclasses.Keyframe_Quaternionf;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static partial class KeyframeExtensions
	{
		public static void SetValues(this IKeyframe_Quaternionf keyframe, UnityVersion version, float time, IQuaternionf value, IQuaternionf weight)
		{
			keyframe.Time = time;
			keyframe.Value.CopyValuesFrom(value);
			// this enum member is version agnostic
			keyframe.TangentMode = TangentMode.FreeSmooth.ToTangent(version);
			keyframe.WeightedMode = (int)WeightedMode.None;
			keyframe.InWeight?.CopyValuesFrom(weight);
			keyframe.OutWeight?.CopyValuesFrom(weight);
		}

		public static void SetValues(this IKeyframe_Quaternionf keyframe, UnityVersion version, float time, IQuaternionf value, float weight)
		{
			keyframe.Time = time;
			keyframe.Value.CopyValuesFrom(value);
			// this enum member is version agnostic
			keyframe.TangentMode = TangentMode.FreeSmooth.ToTangent(version);
			keyframe.WeightedMode = (int)WeightedMode.None;
			keyframe.InWeight?.SetValues(weight, weight, weight, weight);
			keyframe.OutWeight?.SetValues(weight, weight, weight, weight);
		}

		public static void SetValues(this IKeyframe_Quaternionf keyframe, UnityVersion version, float time, IQuaternionf value, float weightX, float weightY, float weightZ, float weightW)
		{
			keyframe.Time = time;
			keyframe.Value.CopyValuesFrom(value);
			// this enum member is version agnostic
			keyframe.TangentMode = TangentMode.FreeSmooth.ToTangent(version);
			keyframe.WeightedMode = (int)WeightedMode.None;
			keyframe.InWeight?.SetValues(weightX, weightY, weightZ, weightW);
			keyframe.OutWeight?.SetValues(weightX, weightY, weightZ, weightW);
		}

		public static void SetValues(this IKeyframe_Quaternionf keyframe, UnityVersion version, float time, IQuaternionf value, IQuaternionf inSlope, IQuaternionf outSlope, IQuaternionf weight)
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

		public static void SetValues(this IKeyframe_Quaternionf keyframe, UnityVersion version, float time, IQuaternionf value, IQuaternionf inSlope, IQuaternionf outSlope, float weight)
		{
			keyframe.Time = time;
			keyframe.Value.CopyValuesFrom(value);
			keyframe.InSlope.CopyValuesFrom(inSlope);
			keyframe.OutSlope.CopyValuesFrom(outSlope);
			// this enum member is version agnostic
			keyframe.TangentMode = TangentMode.FreeFree.ToTangent(version);
			keyframe.WeightedMode = (int)WeightedMode.None;
			keyframe.InWeight?.SetValues(weight, weight, weight, weight);
			keyframe.OutWeight?.SetValues(weight, weight, weight, weight);
		}

		public static void SetValues(this IKeyframe_Quaternionf keyframe, UnityVersion version, float time, IQuaternionf value, IQuaternionf inSlope, IQuaternionf outSlope, float weightX, float weightY, float weightZ, float weightW)
		{
			keyframe.Time = time;
			keyframe.Value.CopyValuesFrom(value);
			keyframe.InSlope.CopyValuesFrom(inSlope);
			keyframe.OutSlope.CopyValuesFrom(outSlope);
			// this enum member is version agnostic
			keyframe.TangentMode = TangentMode.FreeFree.ToTangent(version);
			keyframe.WeightedMode = (int)WeightedMode.None;
			keyframe.InWeight?.SetValues(weightX, weightY, weightZ, weightW);
			keyframe.OutWeight?.SetValues(weightX, weightY, weightZ, weightW);
		}
	}
}
