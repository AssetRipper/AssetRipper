namespace AssetRipper.Reading.Classes
{
	public class StateMachineConstant
    {
        public StateConstant[] m_StateConstantArray;
        public TransitionConstant[] m_AnyStateTransitionConstantArray;
        public SelectorStateConstant[] m_SelectorStateConstantArray;
        public uint m_DefaultState;
        public uint m_MotionSetCount;

        public StateMachineConstant(ObjectReader reader)
        {
            var version = reader.version;

            int numStates = reader.ReadInt32();
            m_StateConstantArray = new StateConstant[numStates];
            for (int i = 0; i < numStates; i++)
            {
                m_StateConstantArray[i] = new StateConstant(reader);
            }

            int numAnyStates = reader.ReadInt32();
            m_AnyStateTransitionConstantArray = new TransitionConstant[numAnyStates];
            for (int i = 0; i < numAnyStates; i++)
            {
                m_AnyStateTransitionConstantArray[i] = new TransitionConstant(reader);
            }

            if (version[0] >= 5) //5.0 and up
            {
                int numSelectors = reader.ReadInt32();
                m_SelectorStateConstantArray = new SelectorStateConstant[numSelectors];
                for (int i = 0; i < numSelectors; i++)
                {
                    m_SelectorStateConstantArray[i] = new SelectorStateConstant(reader);
                }
            }

            m_DefaultState = reader.ReadUInt32();
            m_MotionSetCount = reader.ReadUInt32();
        }
    }
}
