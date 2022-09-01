using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.Converters;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.IO.Files.Streams.MultiFile;
using AssetRipper.IO.Files.Streams.Smart;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.IO.Files.SerializedFiles
{
	/// <summary>
	/// Serialized files contain binary serialized objects and optional run-time type information.
	/// They have file name extensions like .asset, .assets, .sharedAssets but may also have no extension at all
	/// </summary>
	public sealed class SerializedFile : File
	{
		public SerializedFileHeader Header { get; } = new();
		public SerializedFileMetadata Metadata { get; } = new();
		public UnityVersion Version 
		{
			get => Metadata.UnityVersion;
			set => Metadata.UnityVersion = value;
		}
		public BuildTarget Platform
		{
			get => Metadata.TargetPlatform;
			set => Metadata.TargetPlatform = value;
		}

		public IReadOnlyList<FileIdentifier> Dependencies => Metadata.Externals;
		private readonly Dictionary<long, int> m_assetEntryLookup = new();

		public static bool IsSerializedFile(string filePath) => IsSerializedFile(MultiFileStream.OpenRead(filePath));
		public static bool IsSerializedFile(byte[] buffer, int offset, int size) => IsSerializedFile(new MemoryStream(buffer, offset, size, false));
		public static bool IsSerializedFile(Stream stream)
		{
			using EndianReader reader = new EndianReader(stream, EndianType.BigEndian);
			return SerializedFileHeader.IsSerializedFileHeader(reader, stream.Length);
		}

		public ObjectInfo GetAssetEntry(long pathID)
		{
			return Metadata.Object[m_assetEntryLookup[pathID]];
		}

		public override string ToString()
		{
			return NameFixed;
		}

		public EndianType GetEndianType()
		{
			bool swapEndianess = SerializedFileHeader.HasEndianess(Header.Version) ? Header.Endianess : Metadata.SwapEndianess;
			return swapEndianess ? EndianType.BigEndian : EndianType.LittleEndian;
		}

		public override void Read(SmartStream stream)
		{
			using (EndianReader reader = new EndianReader(stream, EndianType.BigEndian))
			{
				Header.Read(reader);
			}
			if (SerializedFileMetadata.IsMetadataAtTheEnd(Header.Version))
			{
				stream.Position = Header.FileSize - Header.MetadataSize;
			}
			Metadata.Read(stream, Header);

			SerializedFileMetadataConverter.CombineFormats(Header.Version, Metadata);
		}

		public override void Write(SmartStream stream)
		{
			throw new NotImplementedException();
		}
	}
}
