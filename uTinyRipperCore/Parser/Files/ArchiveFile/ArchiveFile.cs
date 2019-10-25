using System;
using System.IO;

namespace uTinyRipper.ArchiveFiles
{
	public sealed class ArchiveFile : FileList
	{
		internal ArchiveFile(IFileCollection collection, ArchiveFileScheme scheme)
		{
			if (scheme == null)
			{
				throw new ArgumentNullException(nameof(scheme));
			}

			Header = scheme.Header;
		}

		public static bool IsArchiveFile(string filePath)
		{
			if (!FileMultiStream.Exists(filePath))
			{
				throw new Exception($"Web at path '{filePath}' doesn't exist");
			}

			using (Stream stream = FileMultiStream.OpenRead(filePath))
			{
				return IsArchiveFile(stream);
			}
		}

		public static bool IsArchiveFile(Stream stream)
		{
			return IsArchiveFile(stream, stream.Position, stream.Length - stream.Position);
		}

		public static bool IsArchiveFile(Stream stream, long offset, long size)
		{
			using (PartialStream bundleStream = new PartialStream(stream, offset, size))
			{
				using (EndianReader reader = new EndianReader(bundleStream, EndianType.BigEndian))
				{
					return ArchiveHeader.IsArchiveHeader(reader);
				}
			}
		}

		public static ArchiveFileScheme LoadScheme(string filePath)
		{
			if (!FileUtils.Exists(filePath))
			{
				throw new Exception($"Bundle file at path '{filePath}' doesn't exist");
			}
			string fileName = Path.GetFileNameWithoutExtension(filePath);
			using (SmartStream stream = SmartStream.OpenRead(filePath))
			{
				return ReadScheme(stream, 0, stream.Length, filePath, fileName);
			}
		}

		public static ArchiveFileScheme ReadScheme(SmartStream stream, long offset, long size, string filePath, string fileName)
		{
			return ArchiveFileScheme.ReadScheme(stream, offset, size, filePath, fileName);
		}

		public ArchiveHeader Header { get; private set; }

		private readonly string m_filePath;
	}
}
