using AssetRipper.Core.IO;

namespace AssetRipper.Core.Reading.Classes
{
	public class SelectorTransitionConstant
    {
        public uint m_Destination;
        public ConditionConstant[] m_ConditionConstantArray;

        public SelectorTransitionConstant(ObjectReader reader)
        {
            m_Destination = reader.ReadUInt32();

            int numConditions = reader.ReadInt32();
            m_ConditionConstantArray = new ConditionConstant[numConditions];
            for (int i = 0; i < numConditions; i++)
            {
                m_ConditionConstantArray[i] = new ConditionConstant(reader);
            }
        }
    }
}
