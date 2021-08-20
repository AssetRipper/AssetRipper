using AssetRipper.Core.IO;

namespace AssetRipper.Core.Reading.Classes
{
	public class Node
	{
		public int m_ParentId;
		public int m_AxesId;

		public Node(ObjectReader reader)
		{
			m_ParentId = reader.ReadInt32();
			m_AxesId = reader.ReadInt32();
		}
	}
}
