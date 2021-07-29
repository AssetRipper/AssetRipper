using AssetRipper.IO;
using AssetRipper.IO.Extensions;

namespace AssetRipper.Reading.Classes
{
	public class BlendDirectDataConstant
    {
        public uint[] m_ChildBlendEventIDArray;
        public bool m_NormalizedBlendValues;

        public BlendDirectDataConstant(ObjectReader reader)
        {
            m_ChildBlendEventIDArray = reader.ReadUInt32Array();
            m_NormalizedBlendValues = reader.ReadBoolean();
            reader.AlignStream();
        }
    }
}
