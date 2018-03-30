using System.IO;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.BundleFiles
{
	internal class BundleFileEntry : FileEntry
	{
		public BundleFileEntry(Stream stream, string name, long offset, long size):
			base(stream, name, offset, size)
		{
		}

		public SerializedFile ReadSerializedFile(IAssetCollection collection, string filePath)
		{
			SerializedFile file = new SerializedFile(collection, filePath, Name);
			m_stream.Position = m_offset;
			file.Read(m_stream);
			long read = m_stream.Position - m_offset;
			if (read != m_size)
			{
				//throw new System.Exception($"Read {read} but expected {m_length}");
			}
			return file;
		}

		public long Offset => m_offset;
		public long Size => m_size;
	}
}
