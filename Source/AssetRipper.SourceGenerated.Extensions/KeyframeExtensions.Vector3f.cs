using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions.Enums.Keyframe.TangentMode;
using AssetRipper.SourceGenerated.Subclasses.Keyframe_Vector3f;
using AssetRipper.SourceGenerated.Subclasses.Vector3f;

namespace AssetRipper.SourceGenerated.Extensions;

public static partial class KeyframeExtensions
{
	public static void SetValues(this IKeyframe_Vector3f keyframe, UnityVersion version, float time, IVector3f value, IVector3f weight)
	{
		keyframe.Time = time;
		keyframe.Value.CopyValues(value);
		// this enum member is version agnostic
		keyframe.TangentMode = TangentMode.FreeSmooth.ToTangent(version);
		keyframe.WeightedModeE = WeightedMode.None;
		keyframe.InWeight?.CopyValues(weight);
		keyframe.OutWeight?.CopyValues(weight);
	}

	public static void SetValues(this IKeyframe_Vector3f keyframe, UnityVersion version, float time, IVector3f value, IVector3f inSlope, IVector3f outSlope, IVector3f weight)
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
}
