using AssetRipper.SourceGenerated.Subclasses.QuaternionCurve;

namespace AssetRipper.SourceGenerated.Extensions;

public static class QuaternionCurveExtensions
{
	public static void SetValues(this IQuaternionCurve curve, string path)
	{
		curve.Path = path;
		curve.Curve.SetDefaultRotationOrderAndCurveLoopType();
	}
}
