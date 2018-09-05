using Brotli;
using System;
using System.IO;
using System.IO.Compression;

namespace UtinyRipper.ArchiveFiles
{
	internal class ArchiveFile : IDisposable
	{
		public ArchiveFile(FileCollection fileCollection, string filePath)
		{
			if (fileCollection == null)
			{
				throw new ArgumentNullException(nameof(fileCollection));
			}
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}

			m_fileCollection = fileCollection;
			m_filePath = filePath;
		}

		public static bool IsArchiveFile(string webPath)
		{
			if (!FileMultiStream.Exists(webPath))
			{
				throw new Exception($"Web at path '{webPath}' doesn't exist");
			}

			using (Stream stream = FileMultiStream.OpenRead(webPath))
			{
				return IsArchiveFile(stream);
			}
		}

		public static bool IsArchiveFile(Stream baseStream)
		{
			using (EndianStream stream = new EndianStream(baseStream, baseStream.Position, EndianType.BigEndian))
			{
				long position = stream.BaseStream.Position;
				if (stream.BaseStream.Length - position >= 2)
				{
					ulong magic = stream.ReadUInt16();
					if (magic == GZipMagic)
					{
						stream.BaseStream.Position = position;
						return true;
					}
				}

				if (stream.BaseStream.Length - position >= 0x28)
				{
					stream.BaseStream.Position = position + 0x20;
					ulong magic = (stream.ReadUInt64() & 0xFFFFFFFFFFFF0000) >> 16;
					if (magic == BrotliSignature)
					{
						stream.BaseStream.Position = position;
						return true;
					}
				}

				stream.BaseStream.Position = position;
				return false;
			}
		}

		public void Load(string archivePath)
		{
			if (!FileUtils.Exists(archivePath))
			{
				throw new Exception($"ArchiveFile at path '{archivePath}' doesn't exist");
			}

			FileStream stream = FileUtils.OpenRead(archivePath);
			Read(stream, true);
		}

		public void Read(Stream baseStream)
		{
			Read(baseStream, false);
		}

		public void Dispose()
		{
			Metadata.Dispose();
		}

		private void Read(Stream baseStream, bool isClosable)
		{
			using (EndianStream stream = new EndianStream(baseStream, baseStream.Position, EndianType.BigEndian))
			{
				ulong magic = stream.ReadUInt16();
				stream.BaseStream.Position -= 2;
				if (magic == GZipMagic)
				{
					ReadGZip(stream, isClosable);
				}
				else
				{
					ReadBrotli(stream, isClosable);
				}
			}

			foreach (ArchiveFileEntry entry in Metadata.AssetsEntries)
			{
				entry.ReadFile(m_fileCollection);
			}
		}

		private void ReadGZip(EndianStream stream, bool isClosable)
		{
			MemoryStream memStream = new MemoryStream();
			using (GZipStream gzipStream = new GZipStream(stream.BaseStream, CompressionMode.Decompress))
			{
				gzipStream.CopyTo(memStream);
				memStream.Position = 0;
			}
			if (isClosable)
			{
				stream.Dispose();
			}

			string name = Path.GetFileName(m_filePath);
			ArchiveFileEntry entry = new ArchiveFileEntry(memStream, m_filePath, name, 0, memStream.Length);
			Metadata = new ArchiveMetadata(entry);
		}

		private void ReadBrotli(EndianStream stream, bool isClosable)
		{
			MemoryStream memStream = new MemoryStream();
			using (BrotliInputStream brotliStream = new BrotliInputStream(stream.BaseStream))
			{
				brotliStream.CopyStream(memStream);
				memStream.Position = 0;
			}
			if (isClosable)
			{
				stream.Dispose();
			}

			string name = Path.GetFileName(m_filePath);
			ArchiveFileEntry entry = new ArchiveFileEntry(memStream, m_filePath, name, 0, memStream.Length);
			Metadata = new ArchiveMetadata(entry);
		}

		public ArchiveMetadata Metadata { get; private set; }

		private const ushort GZipMagic = 0x1F8B;
		/// <summary>
		/// "brotli" ascii
		/// </summary>
		private const ulong BrotliSignature = 0x62726F746C69;

		private readonly FileCollection m_fileCollection;
		private readonly string m_filePath;
	}
}
