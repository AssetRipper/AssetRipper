using AssetRipper.IO;
using AssetRipper.IO.Extensions;

namespace AssetRipper.Reading.Classes
{
	public class ConstantClip
    {
        public float[] data;

        public ConstantClip(ObjectReader reader)
        {
            data = reader.ReadSingleArray();
        }
    }
}
