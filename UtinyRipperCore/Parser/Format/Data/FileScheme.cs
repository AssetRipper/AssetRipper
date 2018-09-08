using System;

namespace UtinyRipper
{
	public abstract class FileScheme : IDisposable
	{
		public FileScheme(SmartStream stream, string filePath)
		{
			if(stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}
			m_stream = stream;
			FilePath = filePath;
		}

		~FileScheme()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			m_stream.Dispose();
		}

		public string FilePath { get; }

		protected readonly SmartStream m_stream;

		protected long m_dataOffset;
	}
}
