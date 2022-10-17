using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Subclasses.PPtrCurve;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class PPtrCurveExtensions
	{
		public static void SetValues(this IPPtrCurve curve, AssetCollection collection, string path, string attribute, ClassIDType classID, IMonoScript script)
		{
			curve.Path.String = path;
			curve.Attribute.String = attribute;
			curve.ClassID = (int)classID;
			curve.Script.SetAsset(collection, script);
		}
	}
}
