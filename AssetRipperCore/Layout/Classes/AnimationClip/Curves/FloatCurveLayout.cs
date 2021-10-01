using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.AnimationClip.Curves;
using AssetRipper.Core.Layout.Classes.Misc.Serializable;

namespace AssetRipper.Core.Layout.Classes.AnimationClip.Curves
{
	public sealed class FloatCurveLayout
	{
		public FloatCurveLayout(LayoutInfo info)
		{
			if (info.Version.IsGreaterEqual(2))
			{
				HasScript = true;
			}
		}

		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public bool HasScript { get; }

		public string Name => nameof(FloatCurve);
		public string CurveName => "curve";
		public string AttributeName => "attribute";
		public string PathName => "path";
		public string ClassIDName => "classID";
		public string ScriptName => "script";
	}
}
