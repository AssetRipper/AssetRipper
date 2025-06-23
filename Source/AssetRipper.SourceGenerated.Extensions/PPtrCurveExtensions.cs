using AssetRipper.Assets.Collections;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Subclasses.PPtrCurve;

namespace AssetRipper.SourceGenerated.Extensions;

public static class PPtrCurveExtensions
{
	public static void SetValues(this IPPtrCurve curve, AssetCollection collection, string path, string attribute, ClassIDType classID, IMonoScript script)
	{
		curve.Path = path;
		curve.Attribute = attribute;
		curve.ClassID = (int)classID;
		curve.Script.SetAsset(collection, script);
	}
}
