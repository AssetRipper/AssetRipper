using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.AnimationClip;
using AssetRipper.Core.Classes.AnimationClip.Curves;

namespace AssetRipper.Core.Layout.Classes.AnimationClip.Curves
{
	public sealed class PPtrCurveLayout
	{
		public PPtrCurveLayout(LayoutInfo info)
		{
			if (info.Version.IsGreaterEqual(2017))
			{
				IsAlignCurve = true;
			}
		}


		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public bool IsAlignCurve { get; }

		public string Name => nameof(PPtrCurve);
		public string CurveName => "curve";
		public string AttributeName => "attribute";
		public string PathName => "path";
		public string ClassIDName => "classID";
		public string ScriptName => "script";
	}
}
