using AssetRipper.Classes.Misc;
using AssetRipper.IO;

namespace AssetRipper.Reading.Classes
{
	public class SkeletonPose
    {
        public XForm[] m_X;

        public SkeletonPose(ObjectReader reader)
        {
            int numXforms = reader.ReadInt32();
            m_X = new XForm[numXforms];
            for (int i = 0; i < numXforms; i++)
            {
                m_X[i] = new XForm(reader);
            }
        }
    }
}
