using AssetRipper.Core.IO.Extensions;
using System.IO;

namespace AssetRipper.Core.Reading.Classes
{
	public class StreamedResource
	{
		public string m_Source;
		public long m_Offset; //ulong
		public long m_Size; //ulong

		public StreamedResource(BinaryReader reader)
		{
			m_Source = reader.ReadAlignedString();
			m_Offset = reader.ReadInt64();
			m_Size = reader.ReadInt64();
		}
	}
}
