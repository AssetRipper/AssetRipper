using AssetRipper.IO.Extensions;
using AssetRipper.Math;

namespace AssetRipper.Reading.Classes
{
	public class AABB
    {
        public Vector3f m_Center;
        public Vector3f m_Extent;

        public AABB(ObjectReader reader)
        {
            m_Center = reader.ReadVector3f();
            m_Extent = reader.ReadVector3f();
        }
    }
}
