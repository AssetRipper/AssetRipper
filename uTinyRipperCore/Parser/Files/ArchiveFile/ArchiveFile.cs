using System.IO;
using uTinyRipper.ArchiveFiles;

namespace uTinyRipper
{
	public sealed class ArchiveFile : FileList
	{
		internal ArchiveFile(ArchiveFileScheme scheme) :
			base(scheme.NameOrigin)
		{
			Header = scheme.Header;
		}

		public static bool IsArchiveFile(string filePath)
		{
			using (Stream stream = MultiFileStream.OpenRead(filePath))
			{
				return IsArchiveFile(stream);
			}
		}

		public static bool IsArchiveFile(byte[] buffer, int offset, int size)
		{
			using (MemoryStream stream = new MemoryStream(buffer, offset, size, false))
			{
				return IsArchiveFile(stream);
			}
		}

		public static bool IsArchiveFile(Stream stream)
		{
			using (EndianReader reader = new EndianReader(stream, EndianType.BigEndian))
			{
				return ArchiveHeader.IsArchiveHeader(reader);
			}
		}

		public static ArchiveFileScheme LoadScheme(string filePath)
		{
			string fileName = Path.GetFileNameWithoutExtension(filePath);
			using (Stream stream = MultiFileStream.OpenRead(filePath))
			{
				return ReadScheme(stream, filePath, fileName);
			}
		}

		public static ArchiveFileScheme ReadScheme(byte[] buffer, string filePath, string fileName)
		{
			return ArchiveFileScheme.ReadScheme(buffer, filePath, fileName);
		}

		public static ArchiveFileScheme ReadScheme(Stream stream, string filePath, string fileName)
		{
			return ArchiveFileScheme.ReadScheme(stream, filePath, fileName);
		}

		public ArchiveHeader Header { get; }
	}
}
