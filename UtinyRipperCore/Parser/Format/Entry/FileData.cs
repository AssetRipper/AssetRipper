using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UtinyRipper
{
	internal abstract class FileData<T> : IDisposable
		where T: FileEntry
	{
		protected FileData(Stream stream, bool isClosable)
		{
			m_stream = stream;
			m_isDisposable = isClosable;
		}

		public void Dispose()
		{
			if (m_isDisposable)
			{
				m_stream.Dispose();
				m_isDisposable = false;
			}
		}

		public IEnumerable<T> AssetsEntries => m_entries.Where(t => t.IsSerializedFile);
		public IEnumerable<T> ResourceEntries => m_entries.Where(t => !t.IsSkipFile && !t.IsSerializedFile);
		
		protected Stream m_stream;
		protected IReadOnlyList<T> m_entries;

		private bool m_isDisposable = false;
	}
}
