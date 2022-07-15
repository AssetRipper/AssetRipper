using AssetRipper.SourceGenerated.Subclasses.PPtr_MonoScript_;
using AssetRipper.SourceGenerated.Subclasses.PPtrCurve;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class PPtrCurveExtensions
	{
		public static void SetValues(this IPPtrCurve curve, string path, string attribute, ClassIDType classID, IPPtr_MonoScript_ script)
		{
			curve.Path.String = path;
			curve.Attribute.String = attribute;
			curve.ClassID = (int)classID;
			curve.Script.CopyValues(script);
		}
	}
}
