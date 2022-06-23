using AssetRipper.IO.Files.Entries;
using AssetRipper.IO.Files.Utils;

namespace AssetRipper.IO.Files.ArchiveFiles
{
	public sealed class ArchiveFileEntry : FileEntry
	{
		public ArchiveFileEntry(long offset, long size, string name)
		{
			Offset = offset;
			Size = size;
			NameOrigin = name;
			Name = FilenameUtils.FixFileIdentifier(name);
		}
	}
}
