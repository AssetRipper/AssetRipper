using AssetRipper.Layout.Classes.AnimationClip.Curves;

namespace AssetRipper.Layout.Classes.AnimationClip
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

		public string Name => nameof(AssetRipper.Classes.AnimationClip.AnimationClip);

		public FloatCurveLayout FloatCurve { get; }
		public PPtrCurveLayout PPtrCurve { get; }
		public QuaternionCurveLayout QuaternionCurve { get; }
		public Vector3CurveLayout Vector3Curve { get; }
	}
}
