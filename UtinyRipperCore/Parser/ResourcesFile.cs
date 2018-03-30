using System;
using System.IO;

namespace UtinyRipper
{
	public class ResourcesFile : IDisposable
	{
		public ResourcesFile(string filePath, string fileName, byte[] data):
			this(filePath, fileName, new MemoryStream(data))
		{
		}

		public ResourcesFile(string filePath, string fileName, Stream stream)
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
		}

		public void Dispose()
		{
			if(Stream != null)
			{
				Stream.Dispose();
			}
		}

		/// <summary>
		/// Сontainer's file path (asset bundle or resources file itself)
		/// </summary>
		public string FilePath { get; }
		/// <summary>
		/// Name of resources file in file system or in asset bundle
		/// </summary>
		public string Name { get; }
		public Stream Stream { get; }
	}
}
