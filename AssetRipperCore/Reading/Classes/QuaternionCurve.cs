using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math;

namespace AssetRipper.Core.Reading.Classes
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
