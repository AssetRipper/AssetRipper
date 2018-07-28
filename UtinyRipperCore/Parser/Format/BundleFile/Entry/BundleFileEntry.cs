using System;
using System.IO;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.BundleFiles
{
	internal class BundleFileEntry : FileEntry
	{
		public BundleFileEntry(Stream stream, string filePath, string name, long offset, long size, bool isDisposable):
			base(stream, name, offset, size, isDisposable)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(filePath);
			}
			m_filePath = filePath;
		}

		public SerializedFile ReadSerializedFile(IFileCollection collection, Action<string> requestDependencyCallback)
		{
			SerializedFile file = new SerializedFile(collection, m_filePath, Name);
			m_stream.Position = m_offset;
			file.Read(m_stream, requestDependencyCallback);
			long read = m_stream.Position - m_offset;
			if (read != m_size)
			{
				//throw new System.Exception($"Read {read} but expected {m_length}");
			}
			return file;
		}

		public long Offset => m_offset;
		public long Size => m_size;

		private readonly string m_filePath;
	}
}
