using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Parser.Files.Entries;
using AssetRipper.Core.Parser.Files.Schemes;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using AssetRipper.Core.Parser.Files.WebFiles;
using AssetRipper.Core.Structure.GameStructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace AssetRipper.Core.Parser.Files.ArchiveFiles
{
	public sealed class ArchiveFileScheme : FileScheme
	{
		private ArchiveFileScheme(string filePath, string fileName) : base(filePath, fileName) { }

		internal static ArchiveFileScheme ReadScheme(byte[] buffer, string filePath, string fileName)
		{
			ArchiveFileScheme scheme = new ArchiveFileScheme(filePath, fileName);
			using (MemoryStream stream = new MemoryStream(buffer, 0, buffer.Length, false))
			{
				scheme.ReadScheme(stream);
			}
			return scheme;
		}

		internal static ArchiveFileScheme ReadScheme(Stream stream, string filePath, string fileName)
		{
			ArchiveFileScheme scheme = new ArchiveFileScheme(filePath, fileName);
			scheme.ReadScheme(stream);
			return scheme;
		}

		internal ArchiveFile ReadFile(GameProcessorContext context)
		{
			ArchiveFile archive = new ArchiveFile(this);
			archive.AddFile(context, WebScheme);
			return archive;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		private void ReadScheme(Stream stream)
		{
			byte[] buffer;
			using (EndianReader reader = new EndianReader(stream, EndianType.BigEndian))
			{
				Header.Read(reader);
				buffer = Header.Type switch
				{
					ArchiveType.GZip => ReadGZip(reader),
					ArchiveType.Brotli => ReadBrotli(reader),
					_ => throw new NotSupportedException(Header.Type.ToString()),
				};
			}

			WebScheme = WebFiles.WebFile.ReadScheme(buffer, FilePath);
		}

		private byte[] ReadGZip(EndianReader reader)
		{
			using MemoryStream stream = new MemoryStream();
			using GZipStream gzipStream = new GZipStream(reader.BaseStream, CompressionMode.Decompress);
			gzipStream.CopyTo(stream);
			return stream.ToArray();
		}

		private byte[] ReadBrotli(EndianReader reader)
		{
			using MemoryStream stream = new MemoryStream();
			using BrotliStream brotliStream = new BrotliStream(reader.BaseStream, CompressionMode.Decompress);
			brotliStream.CopyTo(stream);
			return stream.ToArray();
		}

		public override FileEntryType SchemeType => FileEntryType.Archive;
		public override IEnumerable<FileIdentifier> Dependencies => WebScheme.Dependencies;

		public ArchiveHeader Header { get; } = new ArchiveHeader();
		public WebFileScheme WebScheme { get; private set; }
	}
}
