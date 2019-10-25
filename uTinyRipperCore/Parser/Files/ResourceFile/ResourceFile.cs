using System;
using System.IO;
using uTinyRipper.Classes;
using uTinyRipper.ResourceFiles;

namespace uTinyRipper
{
	public class ResourceFile : IResourceFile, IDisposable
	{
		internal ResourceFile(SmartStream stream, long offset, long size, string filePath, string fileName)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}
			if (string.IsNullOrEmpty(fileName))
			{
				throw new ArgumentNullException(nameof(fileName));
			}

			FilePath = filePath;
			Name = fileName;
			m_stream = stream.CreateReference();
			Offset = offset;
			Size = size;
		}

		private ResourceFile(ResourceFile copy) :
			this(copy.m_stream, copy.Offset, copy.Size, copy.FilePath, copy.Name)
		{
		}

		~ResourceFile()
		{
			Dispose(false);
		}

		public static bool IsDefaultResourceFile(string fileName)
		{
			string extension = Path.GetExtension(fileName).ToLower();
			switch (extension)
			{
				case ResourceFileExtension:
				case StreamingFileExtension:
					return true;

				default:
					return false;
			}
		}

		public static ResourceFileScheme LoadScheme(string filePath, string fileName)
		{
			if (!FileMultiStream.Exists(filePath))
			{
				throw new Exception($"Resource file at path '{filePath}' doesn't exist");
			}

			using (SmartStream stream = SmartStream.OpenRead(filePath))
			{
				return ReadScheme(stream, 0, stream.Length, filePath, fileName);
			}
		}

		public static ResourceFileScheme ReadScheme(SmartStream stream, long offset, long size, string filePath, string fileName)
		{
			return ResourceFileScheme.ReadScheme(stream, offset, size, filePath, fileName);
		}

		public ResourceFile CreateCopy()
		{
			return new ResourceFile(this);
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

		protected void Dispose(bool _)
		{
			m_stream.Dispose();
		}

		/// <summary>
		/// Container's file path (asset bundle / web file or resources file itself)
		/// </summary>
		public string FilePath { get; }
		/// <summary>
		/// Name of resources file in file system or in asset bundle / web file
		/// </summary>
		public string Name { get; }
		public Stream Stream => m_stream;
		public long Offset { get; }
		public long Size { get; }

		public const string ResourceFileExtension = ".resource";
		public const string StreamingFileExtension = ".ress";

		private readonly SmartStream m_stream;
	}
}
