using AssetRipper.IO;
using AssetRipper.IO.Extensions;

namespace AssetRipper.Reading.Classes
{
	public class MotionNeighborList
    {
        public uint[] m_NeighborArray;

        public MotionNeighborList(ObjectReader reader)
        {
            m_NeighborArray = reader.ReadUInt32Array();
        }
    }
}
