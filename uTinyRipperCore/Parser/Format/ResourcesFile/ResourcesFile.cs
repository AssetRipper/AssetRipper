using System;
using System.IO;

namespace uTinyRipper
{
	public class ResourcesFile : IDisposable
	{
		internal ResourcesFile(SmartStream stream, string filePath, string fileName, long offset, long size)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}
			if (string.IsNullOrEmpty(fileName))
			{
				throw new ArgumentNullException(nameof(fileName));
			}
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			FilePath = filePath;
			Name = fileName;
			m_stream = stream.CreateReference();
			Offset = offset;
			Size = size;
		}

		private ResourcesFile(ResourcesFile copy):
			this(copy.m_stream, copy.FilePath, copy.Name, copy.Offset, copy.Size)
		{
		}

		~ResourcesFile()
		{
			Dispose(false);
		}
		
		public ResourcesFile CreateReference()
		{
			return new ResourcesFile(this);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public override string ToString()
		{
			return Name ?? base.ToString();
		}

		protected void Dispose(bool disposing)
		{
			m_stream.Dispose();
		}

		/// <summary>
		/// Container's file path (asset bundle or resources file itself)
		/// </summary>
		public string FilePath { get; }
		/// <summary>
		/// Name of resources file in file system or in asset bundle
		/// </summary>
		public string Name { get; }
		public Stream Stream => m_stream;
		public long Offset { get; }
		public long Size { get; }

		private readonly SmartStream m_stream;
	}
}
