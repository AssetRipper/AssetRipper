using AssetRipper.IO.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Reading.Classes
{
	public sealed class AssetBundle : NamedObject
    {
        public PPtr<Classes.Object>[] m_PreloadTable;
        public KeyValuePair<string, AssetInfo>[] m_Container;

        public AssetBundle(ObjectReader reader) : base(reader)
        {
            var m_PreloadTableSize = reader.ReadInt32();
            m_PreloadTable = new PPtr<Classes.Object>[m_PreloadTableSize];
            for (int i = 0; i < m_PreloadTableSize; i++)
            {
                m_PreloadTable[i] = new PPtr<Classes.Object>(reader);
            }

            var m_ContainerSize = reader.ReadInt32();
            m_Container = new KeyValuePair<string, AssetInfo>[m_ContainerSize];
            for (int i = 0; i < m_ContainerSize; i++)
            {
                m_Container[i] = new KeyValuePair<string, AssetInfo>(reader.ReadAlignedString(), new AssetInfo(reader));
            }
        }
    }
}
