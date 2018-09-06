using System.Collections.Generic;

namespace UtinyRipper.ArchiveFiles
{
	public class ArchiveMetadata : Metadata<ArchiveFileEntry>
	{
		public ArchiveMetadata(ArchiveFileEntry entry)
		{
			Entries = new ArchiveFileEntry[] { entry };
		}

		public override IReadOnlyList<ArchiveFileEntry> Entries { get; }
	}
}
