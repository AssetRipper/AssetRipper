using AssetRipper.IO.Extensions;

namespace AssetRipper.Reading.Classes
{
	public class Limit
    {
        public object m_Min;
        public object m_Max;

        public Limit(ObjectReader reader)
        {
            var version = reader.version;
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 4))//5.4 and up
            {
                m_Min = reader.ReadVector3f();
                m_Max = reader.ReadVector3f();
            }
            else
            {
                m_Min = reader.ReadVector4f();
                m_Max = reader.ReadVector4f();
            }
        }
    }
}
