using AssetRipper.Core.IO;

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
