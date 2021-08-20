using AssetRipper.Core.IO;

namespace AssetRipper.Core.Reading.Classes
{
	public class TransitionConstant
    {
        public ConditionConstant[] m_ConditionConstantArray;
        public uint m_DestinationState;
        public uint m_FullPathID;
        public uint m_ID;
        public uint m_UserID;
        public float m_TransitionDuration;
        public float m_TransitionOffset;
        public float m_ExitTime;
        public bool m_HasExitTime;
        public bool m_HasFixedDuration;
        public int m_InterruptionSource;
        public bool m_OrderedInterruption;
        public bool m_Atomic;
        public bool m_CanTransitionToSelf;

        public TransitionConstant(ObjectReader reader)
        {
            var version = reader.version;

            int numConditions = reader.ReadInt32();
            m_ConditionConstantArray = new ConditionConstant[numConditions];
            for (int i = 0; i < numConditions; i++)
            {
                m_ConditionConstantArray[i] = new ConditionConstant(reader);
            }

            m_DestinationState = reader.ReadUInt32();
            if (version[0] >= 5) //5.0 and up
            {
                m_FullPathID = reader.ReadUInt32();
            }

            m_ID = reader.ReadUInt32();
            m_UserID = reader.ReadUInt32();
            m_TransitionDuration = reader.ReadSingle();
            m_TransitionOffset = reader.ReadSingle();
            if (version[0] >= 5) //5.0 and up
            {
                m_ExitTime = reader.ReadSingle();
                m_HasExitTime = reader.ReadBoolean();
                m_HasFixedDuration = reader.ReadBoolean();
                reader.AlignStream();
                m_InterruptionSource = reader.ReadInt32();
                m_OrderedInterruption = reader.ReadBoolean();
            }
            else
            {
                m_Atomic = reader.ReadBoolean();
            }

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 5)) //4.5 and up
            {
                m_CanTransitionToSelf = reader.ReadBoolean();
            }

            reader.AlignStream();
        }
    }
}
