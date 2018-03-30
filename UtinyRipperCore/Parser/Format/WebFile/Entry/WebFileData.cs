using System.Collections.Generic;
using System.IO;

namespace UtinyRipper.WebFiles
{
	internal class WebFileData : FileData<WebFileEntry>
	{
		public WebFileData(Stream stream, bool isClosable, IReadOnlyList<WebFileEntry> entries):
			base(stream, isClosable)
		{
			m_entries = entries;
		}
	}
}
