using AssetRipper.SourceGenerated.Subclasses.FloatCurve;
using AssetRipper.SourceGenerated.Subclasses.PPtr_MonoScript_;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class FloatCurveExtensions
	{
		public static void SetValues(this IFloatCurve curve, string path, string attribute, ClassIDType classID, IPPtr_MonoScript_ script)
		{
			curve.Path.String = path;
			curve.Attribute.String = attribute;
			curve.ClassID = (int)classID;
			curve.Script.CopyValues(script);
			curve.Curve.SetDefaultRotationOrderAndCurveLoopType();
		}
	}
}
