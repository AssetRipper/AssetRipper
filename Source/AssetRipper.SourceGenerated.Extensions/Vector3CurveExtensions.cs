using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.Vector3Curve;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class Vector3CurveExtensions
	{
		public static void SetValues(this IVector3Curve curve, string path)
		{
			curve.Path = path;
			curve.Curve.SetDefaultCurveLoopType();
			curve.Curve.SetRotationOrder();
		}

		public static void SetValues(this IVector3Curve curve, string path, byte customType)
		{
			curve.Path = path;
			curve.Curve.SetDefaultCurveLoopType();
			curve.Curve.SetRotationOrder((RotationOrder)customType);
		}
	}
}
