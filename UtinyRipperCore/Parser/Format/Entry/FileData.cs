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
			IsDisposable = isClosable;
		}

		public void Dispose()
		{
			if (IsDisposable)
			{
				m_stream.Dispose();
				IsDisposable = false;
			}
		}

		public IEnumerable<T> AssetsEntries => m_entries.Where(t => t.IsSerializedFile);
		public IEnumerable<T> ResourceEntries => m_entries.Where(t => !t.IsSkipFile && !t.IsSerializedFile);
		
		protected IReadOnlyList<T> EntriesBase
		{
			get => m_entries;
			set
			{
				m_entries = value;
				foreach(T entry in ResourceEntries)
				{
					IsDisposable = false;
					break;
				}
			}
		}

		protected bool IsDisposable { get; private set; }

		protected Stream m_stream;

		private IReadOnlyList<T> m_entries;

	}
}
