using System;
using System.IO;

namespace UtinyRipper.BundleFiles
{
	internal class BundleFileEntry : FileEntry
	{
		public BundleFileEntry(Stream stream, string filePath, string name, long offset, long size, bool isStreamPermanent) :
			base(stream, filePath, name, offset, size, isStreamPermanent)
		{
		}

		public void ReadFile(FileCollection collection, Action<string> requestDependencyCallback)
		{
			m_stream.Position = m_offset;
			collection.ReadSerializedFile(m_stream, Name, m_filePath, requestDependencyCallback);
			long read = m_stream.Position - m_offset;
			if (read != m_size)
			{
				//throw new System.Exception($"Read {read} but expected {m_length}");
			}
		}

		public long Offset => m_offset;
		public long Size => m_size;
	}
}
