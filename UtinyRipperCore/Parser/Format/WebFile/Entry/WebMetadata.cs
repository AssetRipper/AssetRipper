using System.Collections.Generic;
using System.IO;

namespace UtinyRipper.WebFiles
{
	public class WebMetadata : FileData<WebFileEntry>
	{
		public WebMetadata(Stream stream, bool isClosable, IReadOnlyList<WebFileEntry> entries):
			base(stream, isClosable)
		{
			Entries = entries;
		}

		public override IReadOnlyList<WebFileEntry> Entries { get; }
	}
}
