using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions.Enums.Keyframe.TangentMode;
using AssetRipper.SourceGenerated.Subclasses.Keyframe_Quaternionf;
using System.Numerics;

namespace AssetRipper.SourceGenerated.Extensions;

public static partial class KeyframeExtensions
{
	public static void SetValues(this IKeyframe_Quaternionf keyframe, UnityVersion version, float time, Quaternion value, Quaternion weight)
	{
		keyframe.Time = time;
		keyframe.Value.CopyValues(value);
		// this enum member is version agnostic
		keyframe.TangentMode = TangentMode.FreeSmooth.ToTangent(version);
		keyframe.WeightedModeE = WeightedMode.None;
		keyframe.InWeight?.CopyValues(weight);
		keyframe.OutWeight?.CopyValues(weight);
	}

	public static void SetValues(this IKeyframe_Quaternionf keyframe, UnityVersion version, float time, Quaternion value, float weight)
	{
		keyframe.Time = time;
		keyframe.Value.CopyValues(value);
		// this enum member is version agnostic
		keyframe.TangentMode = TangentMode.FreeSmooth.ToTangent(version);
		keyframe.WeightedModeE = WeightedMode.None;
		keyframe.InWeight?.SetValues(weight, weight, weight, weight);
		keyframe.OutWeight?.SetValues(weight, weight, weight, weight);
	}

	public static void SetValues(this IKeyframe_Quaternionf keyframe, UnityVersion version, float time, Quaternion value, float weightX, float weightY, float weightZ, float weightW)
	{
		keyframe.Time = time;
		keyframe.Value.CopyValues(value);
		// this enum member is version agnostic
		keyframe.TangentMode = TangentMode.FreeSmooth.ToTangent(version);
		keyframe.WeightedModeE = WeightedMode.None;
		keyframe.InWeight?.SetValues(weightX, weightY, weightZ, weightW);
		keyframe.OutWeight?.SetValues(weightX, weightY, weightZ, weightW);
	}

	public static void SetValues(this IKeyframe_Quaternionf keyframe, UnityVersion version, float time, Quaternion value, Quaternion inSlope, Quaternion outSlope, Quaternion weight)
	{
		keyframe.Time = time;
		keyframe.Value.CopyValues(value);
		keyframe.InSlope.CopyValues(inSlope);
		keyframe.OutSlope.CopyValues(outSlope);
		// this enum member is version agnostic
		keyframe.TangentMode = TangentMode.FreeFree.ToTangent(version);
		keyframe.WeightedModeE = WeightedMode.None;
		keyframe.InWeight?.CopyValues(weight);
		keyframe.OutWeight?.CopyValues(weight);
	}

	public static void SetValues(this IKeyframe_Quaternionf keyframe, UnityVersion version, float time, Quaternion value, Quaternion inSlope, Quaternion outSlope, float weight)
	{
		keyframe.Time = time;
		keyframe.Value.CopyValues(value);
		keyframe.InSlope.CopyValues(inSlope);
		keyframe.OutSlope.CopyValues(outSlope);
		// this enum member is version agnostic
		keyframe.TangentMode = TangentMode.FreeFree.ToTangent(version);
		keyframe.WeightedModeE = WeightedMode.None;
		keyframe.InWeight?.SetValues(weight, weight, weight, weight);
		keyframe.OutWeight?.SetValues(weight, weight, weight, weight);
	}

	public static void SetValues(this IKeyframe_Quaternionf keyframe, UnityVersion version, float time, Quaternion value, Quaternion inSlope, Quaternion outSlope, float weightX, float weightY, float weightZ, float weightW)
	{
		keyframe.Time = time;
		keyframe.Value.CopyValues(value);
		keyframe.InSlope.CopyValues(inSlope);
		keyframe.OutSlope.CopyValues(outSlope);
		// this enum member is version agnostic
		keyframe.TangentMode = TangentMode.FreeFree.ToTangent(version);
		keyframe.WeightedModeE = WeightedMode.None;
		keyframe.InWeight?.SetValues(weightX, weightY, weightZ, weightW);
		keyframe.OutWeight?.SetValues(weightX, weightY, weightZ, weightW);
	}
}
