using AssetRipper.Classes;
using AssetRipper.Layout.AnimationClips;

namespace AssetRipper.Layout
{
#warning TODO:
	public sealed class AnimationClipLayout
	{
		public AnimationClipLayout(LayoutInfo info)
		{
			FloatCurve = new FloatCurveLayout(info);
			PPtrCurve = new PPtrCurveLayout(info);
			QuaternionCurve = new QuaternionCurveLayout(info);
			Vector3Curve = new Vector3CurveLayout(info);
		}

		public string Name => nameof(AnimationClip);

		public FloatCurveLayout FloatCurve { get; }
		public PPtrCurveLayout PPtrCurve { get; }
		public QuaternionCurveLayout QuaternionCurve { get; }
		public Vector3CurveLayout Vector3Curve { get; }
	}
}
