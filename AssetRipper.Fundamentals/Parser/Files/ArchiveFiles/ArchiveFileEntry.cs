using AssetRipper.Core.Parser.Files.Entries;
using AssetRipper.Core.Parser.Utils;

namespace AssetRipper.Core.Parser.Files.ArchiveFiles
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
