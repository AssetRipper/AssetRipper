using System;
using System.Collections.Generic;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper
{
	public abstract class FileScheme : IDisposable
	{
		public FileScheme(SmartStream stream, long offset, long size, string filePath, string fileName)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}
			m_stream = stream.CreateReference();
			m_offset = offset;
			m_size = size;
			FilePath = filePath;
			NameOrigin = fileName;
			Name = FilenameUtils.FixFileIdentifier(fileName);

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

		public abstract bool ContainsFile(string fileName);

		public override string ToString()
		{
			return Name == null ? base.ToString() : $"T:{SchemeType} N:'{Name}'";
		}

		protected virtual void Dispose(bool disposing)
		{
			m_stream.Dispose();
		}

		public string FilePath { get; }
		public string NameOrigin { get; }
		public string Name { get; }

		public abstract FileEntryType SchemeType { get; }
		public abstract IEnumerable<FileIdentifier> Dependencies { get; }

		protected readonly SmartStream m_stream;
		protected readonly long m_offset; 
		protected readonly long m_size;
	}
}
