using AssetRipper.Core.Classes.Misc.KeyframeTpl;
using AssetRipper.Core.Classes.Misc.KeyframeTpl.TangentMode;
using AssetRipper.SourceGenerated.Subclasses.Keyframe_float;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static partial class KeyframeExtensions
	{
		public const float DefaultFloatWeight = (float)(1.0 / 3.0);

		public static void SetValues(this IKeyframe_float keyframe, UnityVersion version, float time, float value, float weight)
		{
			keyframe.Time = time;
			keyframe.Value = value;
			// this enum member is version agnostic
			keyframe.TangentMode = TangentMode.FreeSmooth.ToTangent(version);
			keyframe.WeightedMode = (int)WeightedMode.None;
			keyframe.InWeight = weight;
			keyframe.OutWeight = weight;
		}

		public static void SetValues(this IKeyframe_float keyframe, UnityVersion version, float time, float value, float inSlope, float outSlope, float weight)
		{
			keyframe.Time = time;
			keyframe.Value = value;
			keyframe.InSlope = inSlope;
			keyframe.OutSlope = outSlope;
			// this enum member is version agnostic
			keyframe.TangentMode = TangentMode.FreeFree.ToTangent(version);
			keyframe.WeightedMode = (int)WeightedMode.None;
			keyframe.InWeight = weight;
			keyframe.OutWeight = weight;
		}
	}
}
