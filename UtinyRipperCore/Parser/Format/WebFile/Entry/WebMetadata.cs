using System.Collections.Generic;

namespace UtinyRipper.WebFiles
{
	public class WebMetadata : Metadata<WebFileEntry>
	{
		internal WebMetadata(IReadOnlyList<WebFileEntry> entries)
		{
			Entries = entries;
		}

		public override IReadOnlyList<WebFileEntry> Entries { get; }
	}
}
