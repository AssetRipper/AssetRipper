using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.Converters;
using AssetRipper.IO.Files.Entries;
using AssetRipper.IO.Files.Extensions;
using AssetRipper.IO.Files.Schemes;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.IO.Files.Streams.Smart;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.IO.Files.SerializedFiles
{
	public sealed class SerializedFileScheme : FileScheme
	{
		private SerializedFileScheme(byte[] buffer, string filePath, string fileName) : base(filePath, fileName)
		{
			Stream = new MemoryStream(buffer, 0, buffer.Length, false);
		}

		private SerializedFileScheme(SmartStream stream, string filePath, string fileName) : base(filePath, fileName)
		{
			if (stream.Length <= int.MaxValue)
			{
				byte[] buffer = new byte[stream.Length];
				stream.ReadBuffer(buffer, 0, buffer.Length);
				Stream = new MemoryStream(buffer, 0, buffer.Length, false);
			}
			else
			{
				Stream = stream.CreateReference();
			}
		}

		internal static SerializedFileScheme ReadSceme(byte[] buffer, string filePath, string fileName)
		{
			SerializedFileScheme scheme = new SerializedFileScheme(buffer, filePath, fileName);
			scheme.ReadScheme();
			return scheme;
		}

		internal static SerializedFileScheme ReadSceme(SmartStream stream, string filePath, string fileName)
		{
			SerializedFileScheme scheme = new SerializedFileScheme(stream, filePath, fileName);
			scheme.ReadScheme();
			return scheme;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (Stream != null)
			{
				Stream.Dispose();
				Stream = null!;
			}
		}

		private void ReadScheme()
		{
			using (EndianReader reader = new EndianReader(Stream, EndianType.BigEndian))
			{
				Header.Read(reader);
			}
			if (SerializedFileMetadata.IsMetadataAtTheEnd(Header.Version))
			{
				Stream.Position = Header.FileSize - Header.MetadataSize;
			}
			Metadata.Read(Stream, Header);

			SerializedFileMetadataConverter.CombineFormats(Header.Version, Metadata);
		}

		public override FileEntryType SchemeType => FileEntryType.Serialized;
		public override IEnumerable<FileIdentifier> Dependencies => Metadata.Externals;

		public SerializedFileHeader Header { get; } = new SerializedFileHeader();
		public SerializedFileMetadata Metadata { get; } = new SerializedFileMetadata();
		public Stream Stream { get; private set; }
	}
}
