using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math;

namespace AssetRipper.Core.Reading.Classes
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
