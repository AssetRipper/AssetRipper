using AssetRipper.Core.Classes.AnimationClip.Curves;
using AssetRipper.Core.Layout.Classes.Misc.Serializable;
using AssetRipper.Core.Math;

namespace AssetRipper.Core.Layout.Classes.AnimationClip.Curves
{
	public sealed class Vector3CurveLayout
	{
		public Vector3CurveLayout(LayoutInfo info) { }

		public string Name => nameof(Vector3Curve);
		public string CurveName => "curve";
		public string PathName => "path";
	}
}
