using AssetRipper.IO;

namespace AssetRipper.Reading.Classes
{
	public class SkeletonPose
    {
        public xform[] m_X;

        public SkeletonPose(ObjectReader reader)
        {
            int numXforms = reader.ReadInt32();
            m_X = new xform[numXforms];
            for (int i = 0; i < numXforms; i++)
            {
                m_X[i] = new xform(reader);
            }
        }
    }
}
