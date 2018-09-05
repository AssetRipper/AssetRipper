using System.IO;

namespace UtinyRipper.ArchiveFiles
{
	internal class ArchiveFileEntry : FileEntry
	{
		public ArchiveFileEntry(Stream stream, string filePath, string name, long offset, long size) :
			base(stream, filePath, name, offset, size, true)
		{
		}

		public void ReadFile(FileCollection collection)
		{
			m_stream.Position = m_offset;
			collection.Read(m_stream, Name, m_filePath);
		}
	}
}
