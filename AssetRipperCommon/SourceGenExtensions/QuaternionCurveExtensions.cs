using AssetRipper.SourceGenerated.Subclasses.QuaternionCurve;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class QuaternionCurveExtensions
	{
		public static void SetValues(this IQuaternionCurve curve, string path)
		{
			curve.Path.String = path;
			curve.Curve.SetDefaultRotationOrderAndCurveLoopType();
		}
	}
}
