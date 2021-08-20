using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Core.Reading.Classes
{
	public class ResourceManager : Classes.Object
	{
		public KeyValuePair<string, PPtr<Classes.Object>>[] m_Container;

		public ResourceManager(ObjectReader reader) : base(reader)
		{
			var m_ContainerSize = reader.ReadInt32();
			m_Container = new KeyValuePair<string, PPtr<Classes.Object>>[m_ContainerSize];
			for (int i = 0; i < m_ContainerSize; i++)
			{
				m_Container[i] = new KeyValuePair<string, PPtr<Classes.Object>>(reader.ReadAlignedString(), new PPtr<Classes.Object>(reader));
			}
		}
	}
}
