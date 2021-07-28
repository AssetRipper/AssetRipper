namespace AssetRipper.Reading.Classes
{
	public class SkeletonMask
    {
        public SkeletonMaskElement[] m_Data;

        public SkeletonMask(ObjectReader reader)
        {
            int numElements = reader.ReadInt32();
            m_Data = new SkeletonMaskElement[numElements];
            for (int i = 0; i < numElements; i++)
            {
                m_Data[i] = new SkeletonMaskElement(reader);
            }
        }
    }
}
