using AssetRipper.SourceGenerated.Subclasses.Vector3Curve;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class Vector3CurveExtensions
	{
		public static void SetValues(this IVector3Curve curve, string path)
		{
			curve.Path.String = path;
			curve.Curve.SetDefaultRotationOrderAndCurveLoopType();
		}
	}
}
