using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Extensions;

namespace AssetRipper.Core.Reading.Classes
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
