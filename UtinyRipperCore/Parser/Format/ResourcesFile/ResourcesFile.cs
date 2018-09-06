using System;

namespace UtinyRipper
{
	public class ResourcesFile : IDisposable
	{
		internal ResourcesFile(SmartStream stream, string filePath, string fileName)
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
			Stream = stream.CreateReference();
			m_basePisition = stream.Position;
		}

		internal ResourcesFile(ResourcesFile copy)
		{
			FilePath = copy.FilePath;
			Name = copy.Name;
			Stream = copy.Stream.CreateReference();
			m_basePisition = copy.m_basePisition;
		}

		~ResourcesFile()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Dispose(bool disposing)
		{
			Stream.Dispose();
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
		public SmartStream Stream { get; }
		public long Position
		{
			set => Stream.Position = m_basePisition + value;
		}

		private long m_basePisition = 0;
	}
}
