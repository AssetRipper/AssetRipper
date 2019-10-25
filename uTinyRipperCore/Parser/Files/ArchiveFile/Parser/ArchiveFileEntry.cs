namespace uTinyRipper.ArchiveFiles
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
