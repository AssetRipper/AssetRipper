using AssetRipper.Classes.AnimatorController.Constants;
using AssetRipper.IO;
using AssetRipper.IO.Extensions;

namespace AssetRipper.Reading.Classes
{
	public class StateConstant
    {
        public TransitionConstant[] m_TransitionConstantArray;
        public int[] m_BlendTreeConstantIndexArray;
        public LeafInfoConstant[] m_LeafInfoArray;
        public BlendTreeConstant[] m_BlendTreeConstantArray;
        public uint m_NameID;
        public uint m_PathID;
        public uint m_FullPathID;
        public uint m_TagID;
        public uint m_SpeedParamID;
        public uint m_MirrorParamID;
        public uint m_CycleOffsetParamID;
        public float m_Speed;
        public float m_CycleOffset;
        public bool m_IKOnFeet;
        public bool m_WriteDefaultValues;
        public bool m_Loop;
        public bool m_Mirror;

        public StateConstant(ObjectReader reader)
        {
            var version = reader.version;

            int numTransistions = reader.ReadInt32();
            m_TransitionConstantArray = new TransitionConstant[numTransistions];
            for (int i = 0; i < numTransistions; i++)
            {
                m_TransitionConstantArray[i] = new TransitionConstant(reader);
            }

            m_BlendTreeConstantIndexArray = reader.ReadInt32Array();

            if (version[0] < 5 || (version[0] == 5 && version[1] < 2)) //5.2 down
            {
                int numInfos = reader.ReadInt32();
                m_LeafInfoArray = new LeafInfoConstant[numInfos];
                for (int i = 0; i < numInfos; i++)
                {
                    m_LeafInfoArray[i] = new LeafInfoConstant(reader);
                }
            }

            int numBlends = reader.ReadInt32();
            m_BlendTreeConstantArray = new BlendTreeConstant[numBlends];
            for (int i = 0; i < numBlends; i++)
            {
                m_BlendTreeConstantArray[i] = new BlendTreeConstant(reader);
            }

            m_NameID = reader.ReadUInt32();
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                m_PathID = reader.ReadUInt32();
            }
            if (version[0] >= 5) //5.0 and up
            {
                m_FullPathID = reader.ReadUInt32();
            }

            m_TagID = reader.ReadUInt32();
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 1)) //5.1 and up
            {
                m_SpeedParamID = reader.ReadUInt32();
                m_MirrorParamID = reader.ReadUInt32();
                m_CycleOffsetParamID = reader.ReadUInt32();
            }

            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 2)) //2017.2 and up
            {
                var m_TimeParamID = reader.ReadUInt32();
            }

            m_Speed = reader.ReadSingle();
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 1)) //4.1 and up
            {
                m_CycleOffset = reader.ReadSingle();
            }
            m_IKOnFeet = reader.ReadBoolean();
            if (version[0] >= 5) //5.0 and up
            {
                m_WriteDefaultValues = reader.ReadBoolean();
            }

            m_Loop = reader.ReadBoolean();
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 1)) //4.1 and up
            {
                m_Mirror = reader.ReadBoolean();
            }

            reader.AlignStream();
        }
    }
}
