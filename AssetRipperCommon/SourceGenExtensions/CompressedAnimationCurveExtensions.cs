using AssetRipper.Core.IO;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.SourceGenerated.Subclasses.CompressedAnimationCurve;
using AssetRipper.SourceGenerated.Subclasses.Keyframe_Quaternionf;
using AssetRipper.SourceGenerated.Subclasses.QuaternionCurve;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class CompressedAnimationCurveExtensions
	{
		public static void Unpack(this CompressedAnimationCurve compressedAnimationCurve, UnityVersion version, IQuaternionCurve destinationCurve)
		{
			int[] timesValues = compressedAnimationCurve.Times.UnpackInts();
			float[] times = new float[timesValues.Length];
			for (int i = 0; i < times.Length; i++)
			{
				times[i] = timesValues[i] * 0.01f;
			}
			Quaternionf[] rotations = compressedAnimationCurve.Values.Unpack();
			float[] slopes = compressedAnimationCurve.Slopes.Unpack();


			AccessListBase<IKeyframe_Quaternionf> keyframes = destinationCurve.Curve.Curve; //new KeyframeTpl<Quaternionf>[rotations.Length];
			keyframes.Clear();
			keyframes.Capacity = rotations.Length;

			for (int i = 0, j = 4; i < rotations.Length; i++, j += 4)
			{
				float time = times[i];
				Quaternionf rotation = rotations[i];
				Quaternionf inSlope = new Quaternionf(slopes[j - 4], slopes[j - 3], slopes[j - 2], slopes[j - 1]);
				Quaternionf outSlope = new Quaternionf(slopes[j + 0], slopes[j + 1], slopes[j + 2], slopes[j + 3]);
				IKeyframe_Quaternionf keyframe = keyframes.AddNew();
				keyframe.SetValues(version, time, rotation, inSlope, outSlope, 1.0f / 3.0f);
			}
			destinationCurve.Path.CopyValues(compressedAnimationCurve.Path);
			destinationCurve.Curve.SetDefaultRotationOrderAndCurveLoopType();
			destinationCurve.Curve.PreInfinity = compressedAnimationCurve.PreInfinity;
			destinationCurve.Curve.PostInfinity = compressedAnimationCurve.PostInfinity;
		}
	}
}
