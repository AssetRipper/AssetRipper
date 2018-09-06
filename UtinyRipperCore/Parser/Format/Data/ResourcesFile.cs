using System;
using System.IO;

namespace UtinyRipper
{
	public class ResourcesFile : IDisposable
	{
		public ResourcesFile(Stream stream, string filePath, string fileName)
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
			Stream = stream;
			m_basePisition = stream.Position;
		}
		
		public void Dispose()
		{
			if(Stream != null)
			{
				Stream.Dispose();
			}
		}

		public override string ToString()
		{
			return Name == null ? base.ToString() : Name;
		}

		/// <summary>
		/// Container's file path (asset bundle or resources file itself)
		/// </summary>
		public string FilePath { get; }
		/// <summary>
		/// Name of resources file in file system or in asset bundle
		/// </summary>
		public string Name { get; }
		public Stream Stream { get; }
		public long Position
		{
			set => Stream.Position = m_basePisition + value;
		}

		private long m_basePisition = 0;
	}
}
