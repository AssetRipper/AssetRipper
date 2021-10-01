using AssetRipper.Core.Classes.AnimationClip.Curves;
using AssetRipper.Core.Layout.Classes.Misc.Serializable;
using AssetRipper.Core.Math;

namespace AssetRipper.Core.Layout.Classes.AnimationClip.Curves
{
	public sealed class QuaternionCurveLayout
	{
		public QuaternionCurveLayout(LayoutInfo info) { }

		public string Name => nameof(QuaternionCurve);
		public string CurveName => "curve";
		public string PathName => "path";
	}
}
