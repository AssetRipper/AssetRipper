using AssetRipper.Assets.Collections;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Subclasses.FloatCurve;

namespace AssetRipper.SourceGenerated.Extensions;

public static class FloatCurveExtensions
{
	public static void SetValues(this IFloatCurve curve, AssetCollection collection, string path, string attribute, ClassIDType classID, IMonoScript script)
	{
		curve.Path = path;
		curve.Attribute = attribute;
		curve.ClassID = (int)classID;
		curve.Script.SetAsset(collection, script);
		curve.Curve.SetDefaultRotationOrderAndCurveLoopType();
	}
}
