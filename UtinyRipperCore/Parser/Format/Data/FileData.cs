using System;
using System.Collections.Generic;
using System.IO;

namespace UtinyRipper
{
	public abstract class FileData<T> : IDisposable
		where T: FileEntry
	{
		protected FileData(Stream stream, bool isClosable)
		{
			m_stream = stream;
			IsDisposable = false; // = isClosable;
		}

		public void Dispose()
		{
			if (IsDisposable)
			{
				m_stream.Dispose();
				IsDisposable = false;
			}
		}

		public abstract IReadOnlyList<T> Entries { get; }
		
		protected bool IsDisposable { get; private set; }

		protected Stream m_stream;

	}
}
