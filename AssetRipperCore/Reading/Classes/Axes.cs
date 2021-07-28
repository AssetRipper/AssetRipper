using AssetRipper.IO.Extensions;
using AssetRipper.Math;

namespace AssetRipper.Reading.Classes
{
	public class Axes
    {
        public Vector4f m_PreQ;
        public Vector4f m_PostQ;
        public object m_Sgn;
        public Limit m_Limit;
        public float m_Length;
        public uint m_Type;

        public Axes(ObjectReader reader)
        {
            var version = reader.version;
            m_PreQ = reader.ReadVector4f();
            m_PostQ = reader.ReadVector4f();
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 4)) //5.4 and up
            {
                m_Sgn = reader.ReadVector3f();
            }
            else
            {
                m_Sgn = reader.ReadVector4f();
            }
            m_Limit = new Limit(reader);
            m_Length = reader.ReadSingle();
            m_Type = reader.ReadUInt32();
        }
    }
}
