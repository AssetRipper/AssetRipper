using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO;

namespace AssetRipper.Core.Reading.Classes
{
	public class Handle
    {
        public XForm m_X = new();
		public uint m_ParentHumanIndex;
        public uint m_ID;

        public Handle(ObjectReader reader)
        {
            m_X = new XForm(reader);
            m_ParentHumanIndex = reader.ReadUInt32();
            m_ID = reader.ReadUInt32();
        }
    }
}
