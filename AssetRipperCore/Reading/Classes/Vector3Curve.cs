using AssetRipper.IO.Extensions;
using AssetRipper.Math;

namespace AssetRipper.Reading.Classes
{
	public class Vector3Curve
    {
        public AnimationCurve<Vector3f> curve;
        public string path;

        public Vector3Curve(ObjectReader reader)
        {
            curve = new AnimationCurve<Vector3f>(reader, reader.ReadVector3f);
            path = reader.ReadAlignedString();
        }
    }
}
