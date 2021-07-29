using AssetRipper.IO;
using AssetRipper.IO.Extensions;

namespace AssetRipper.Reading.Classes
{
	public class Blend1dDataConstant // wrong labeled
    {
        public float[] m_ChildThresholdArray;

        public Blend1dDataConstant(ObjectReader reader)
        {
            m_ChildThresholdArray = reader.ReadSingleArray();
        }
    }
}
