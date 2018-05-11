using System.Collections.Generic;
using System.IO;

namespace UtinyRipper.WebFiles
{
	internal class WebMetadata : FileData<WebFileEntry>
	{
		public WebMetadata(Stream stream, bool isClosable, IReadOnlyList<WebFileEntry> entries):
			base(stream, isClosable)
		{
			m_entries = entries;
		}
	}
}
