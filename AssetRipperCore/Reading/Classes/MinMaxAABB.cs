using AssetRipper.IO.Extensions;
using AssetRipper.Math;
using System.IO;

namespace AssetRipper.Reading.Classes
{
	public class MinMaxAABB
    {
        public Vector3f m_Min;
        public Vector3f m_Max;

        public MinMaxAABB(BinaryReader reader)
        {
            m_Min = reader.ReadVector3f();
            m_Max = reader.ReadVector3f();
        }
    }
}
