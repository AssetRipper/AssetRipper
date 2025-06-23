using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.Vector3Curve;

namespace AssetRipper.SourceGenerated.Extensions;

public static class Vector3CurveExtensions
{
	public static void SetValues(this IVector3Curve curve, string path)
	{
		curve.Path = path;
		curve.Curve.SetDefaultRotationOrderAndCurveLoopType();
	}

	public static void SetValues(this IVector3Curve curve, string path, RotationOrder rotationOrder)
	{
		curve.Path = path;
		curve.Curve.SetDefaultRotationOrderAndCurveLoopType();
		curve.Curve.RotationOrderE = rotationOrder;
	}
}
