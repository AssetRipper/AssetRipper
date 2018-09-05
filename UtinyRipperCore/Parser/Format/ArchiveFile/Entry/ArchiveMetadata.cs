namespace UtinyRipper.ArchiveFiles
{
	internal class ArchiveMetadata : FileData<ArchiveFileEntry>
	{
		public ArchiveMetadata(ArchiveFileEntry entry) :
			base(null, false)
		{
			EntriesBase = new ArchiveFileEntry[] { entry };
		}
	}
}
