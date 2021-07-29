using AssetRipper.IO;
using AssetRipper.IO.Extensions;
using AssetRipper.Math;

namespace AssetRipper.Reading.Classes
{
	public class QuaternionCurve
    {
        public AnimationCurve<Quaternionf> curve;
        public string path;

        public QuaternionCurve(ObjectReader reader)
        {
            curve = new AnimationCurve<Quaternionf>(reader, reader.ReadQuaternionf);
            path = reader.ReadAlignedString();
        }
    }
}
