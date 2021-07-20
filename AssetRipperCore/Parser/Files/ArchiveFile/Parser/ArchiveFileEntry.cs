using AssetRipper.Parser.Files.File.Parser;
using AssetRipper.Parser.Utils;

namespace AssetRipper.Parser.Files.ArchiveFile.Parser
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
