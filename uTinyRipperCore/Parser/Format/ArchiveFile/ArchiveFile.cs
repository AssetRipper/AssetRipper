using Brotli;
using System;
using System.IO;
using System.IO.Compression;

namespace uTinyRipper.ArchiveFiles
{
	public sealed class ArchiveFile : IDisposable
	{
		private ArchiveFile(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}

			m_filePath = filePath;
		}

		~ArchiveFile()
		{
			Dispose(false);
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
			using (EndianReader reader = new EndianReader(baseStream, baseStream.Position, EndianType.BigEndian))
			{
				long position = reader.BaseStream.Position;
				if (reader.BaseStream.Length - position >= 2)
				{
					ulong magic = reader.ReadUInt16();
					if (magic == GZipMagic)
					{
						reader.BaseStream.Position = position;
						return true;
					}
				}

				if (reader.BaseStream.Length - position >= 0x28)
				{
					reader.BaseStream.Position = position + 0x20;
					ulong magic = (reader.ReadUInt64() & 0xFFFFFFFFFFFF0000) >> 16;
					if (magic == BrotliSignature)
					{
						reader.BaseStream.Position = position;
						return true;
					}
				}

				reader.BaseStream.Position = position;
				return false;
			}
		}

		public static ArchiveFile Load(string archivePath)
		{
			ArchiveFile archive = new ArchiveFile(archivePath);
			archive.Load();
			return archive;
		}

		public static ArchiveFile Read(SmartStream stream, string archivePath)
		{
			ArchiveFile archive = new ArchiveFile(archivePath);
			archive.Read(stream);
			return archive;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if(Metadata != null)
			{
				Metadata.Dispose();
			}
		}

		private void Load()
		{
			if (!FileUtils.Exists(m_filePath))
			{
				throw new Exception($"ArchiveFile at path '{m_filePath}' doesn't exist");
			}

			using (FileStream stream = File.OpenRead(m_filePath))
			{
				Read(stream);
			}
		}

		private void Read(Stream stream)
		{
			using (EndianReader reader = new EndianReader(stream, stream.Position, EndianType.BigEndian))
			{
				ulong magic = reader.ReadUInt16();
				reader.BaseStream.Position -= 2;
				if (magic == GZipMagic)
				{
					ReadGZip(reader);
				}
				else
				{
					ReadBrotli(reader);
				}
			}
		}

		private void ReadGZip(EndianReader reader)
		{
			using (SmartStream stream = SmartStream.CreateMemory())
			{
				using (GZipStream gzipStream = new GZipStream(reader.BaseStream, CompressionMode.Decompress))
				{
					gzipStream.CopyTo(stream);
					stream.Position = 0;
				}

				string name = Path.GetFileName(m_filePath);
				ArchiveFileEntry entry = new ArchiveFileEntry(stream, m_filePath, name, 0, stream.Length);
				Metadata = new ArchiveMetadata(entry);
			}
		}

		private void ReadBrotli(EndianReader reader)
		{
			using (SmartStream stream = SmartStream.CreateMemory())
			{
				using (BrotliInputStream brotliStream = new BrotliInputStream(reader.BaseStream))
				{
					brotliStream.CopyStream(stream);
					stream.Position = 0;
				}

				string name = Path.GetFileName(m_filePath);
				ArchiveFileEntry entry = new ArchiveFileEntry(stream, m_filePath, name, 0, stream.Length);
				Metadata = new ArchiveMetadata(entry);
			}
		}

		public ArchiveMetadata Metadata { get; private set; }

		private const ushort GZipMagic = 0x1F8B;
		/// <summary>
		/// "brotli" ascii
		/// </summary>
		private const ulong BrotliSignature = 0x62726F746C69;

		private readonly string m_filePath;
	}
}
