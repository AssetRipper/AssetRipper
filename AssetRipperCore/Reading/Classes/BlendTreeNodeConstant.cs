using AssetRipper.IO;
using AssetRipper.IO.Extensions;

namespace AssetRipper.Reading.Classes
{
	public class BlendTreeNodeConstant
    {
        public uint m_BlendType;
        public uint m_BlendEventID;
        public uint m_BlendEventYID;
        public uint[] m_ChildIndices;
        public float[] m_ChildThresholdArray;
        public Blend1dDataConstant m_Blend1dData;
        public Blend2dDataConstant m_Blend2dData;
        public BlendDirectDataConstant m_BlendDirectData;
        public uint m_ClipID;
        public uint m_ClipIndex;
        public float m_Duration;
        public float m_CycleOffset;
        public bool m_Mirror;

        public BlendTreeNodeConstant(ObjectReader reader)
        {
            var version = reader.version;

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 1)) //4.1 and up
            {
                m_BlendType = reader.ReadUInt32();
            }
            m_BlendEventID = reader.ReadUInt32();
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 1)) //4.1 and up
            {
                m_BlendEventYID = reader.ReadUInt32();
            }
            m_ChildIndices = reader.ReadUInt32Array();
            if (version[0] < 4 || (version[0] == 4 && version[1] < 1)) //4.1 down
            {
                m_ChildThresholdArray = reader.ReadSingleArray();
            }

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 1)) //4.1 and up
            {
                m_Blend1dData = new Blend1dDataConstant(reader);
                m_Blend2dData = new Blend2dDataConstant(reader);
            }

            if (version[0] >= 5) //5.0 and up
            {
                m_BlendDirectData = new BlendDirectDataConstant(reader);
            }

            m_ClipID = reader.ReadUInt32();
            if (version[0] == 4 && version[1] >= 5) //4.5 - 5.0
            {
                m_ClipIndex = reader.ReadUInt32();
            }

            m_Duration = reader.ReadSingle();

            if (version[0] > 4
                || (version[0] == 4 && version[1] > 1)
                || (version[0] == 4 && version[1] == 1 && version[2] >= 3)) //4.1.3 and up
            {
                m_CycleOffset = reader.ReadSingle();
                m_Mirror = reader.ReadBoolean();
                reader.AlignStream();
            }
        }
    }
}
