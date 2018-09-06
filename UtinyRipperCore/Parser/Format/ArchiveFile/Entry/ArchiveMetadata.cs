using System.Collections.Generic;

namespace UtinyRipper.ArchiveFiles
{
	public class ArchiveMetadata : FileData<ArchiveFileEntry>
	{
		public ArchiveMetadata(ArchiveFileEntry entry) :
			base(null, false)
		{
			Entries = new ArchiveFileEntry[] { entry };
		}

		public override IReadOnlyList<ArchiveFileEntry> Entries { get; }
	}
}
