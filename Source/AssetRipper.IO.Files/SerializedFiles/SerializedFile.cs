using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.Converters;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.IO.Files.Streams.Smart;
using AssetRipper.IO.Files.Utils;

namespace AssetRipper.IO.Files.SerializedFiles
{
	/// <summary>
	/// Serialized files contain binary serialized objects and optional run-time type information.
	/// They have file name extensions like .asset, .assets, .sharedAssets but may also have no extension at all
	/// </summary>
	public sealed class SerializedFile : FileBase
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
		public TransferInstructionFlags Flags => GetFlags(Header, Metadata, Name, FilePath);
		public EndianType EndianType => GetEndianType(Header, Metadata);
		public ReadOnlySpan<FileIdentifier> Dependencies => Metadata.Externals;

		private static TransferInstructionFlags GetFlags(SerializedFileHeader header, SerializedFileMetadata metadata, string name, string filePath)
		{
			TransferInstructionFlags flags;
			if (SerializedFileMetadata.HasPlatform(header.Version) && metadata.TargetPlatform == BuildTarget.NoTarget)
			{
				if (filePath.EndsWith(".unity", StringComparison.Ordinal))
				{
					flags = TransferInstructionFlags.SerializeEditorMinimalScene;
				}
				else
				{
					flags = TransferInstructionFlags.NoTransferInstructionFlags;
				}
			}
			else
			{
				flags = TransferInstructionFlags.SerializeGameRelease;
			}

			if (FilenameUtils.IsEngineResource(name) || (header.Version < FormatVersion.Unknown_10 && FilenameUtils.IsBuiltinExtra(name)))
			{
				flags |= TransferInstructionFlags.IsBuiltinResourcesFile;
			}
			if (header.Endianess || metadata.SwapEndianess)
			{
				flags |= TransferInstructionFlags.SwapEndianess;
			}
			return flags;
		}

		private static EndianType GetEndianType(SerializedFileHeader header, SerializedFileMetadata metadata)
		{
			bool swapEndianess = SerializedFileHeader.HasEndianess(header.Version) ? header.Endianess : metadata.SwapEndianess;
			return swapEndianess ? EndianType.BigEndian : EndianType.LittleEndian;
		}

		public static bool IsSerializedFile(Stream stream)
		{
			using EndianReader reader = new EndianReader(stream, EndianType.BigEndian);
			return SerializedFileHeader.IsSerializedFileHeader(reader, stream.Length);
		}

		public override string ToString()
		{
			return NameFixed;
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

			foreach (ObjectInfo objectInfo in Metadata.Object)
			{
				stream.Position = Header.DataOffset + objectInfo.ByteStart;
				byte[] objectData = new byte[objectInfo.ByteSize];
				stream.ReadExactly(objectData);
				objectInfo.ObjectData = objectData;
			}
		}

		public override void Write(Stream stream)
		{
			throw new NotImplementedException();
		}

		public static SerializedFile FromFile(string filePath)
		{
			string fileName = Path.GetFileName(filePath);
			SmartStream stream = SmartStream.OpenRead(filePath);
			return SerializedFileScheme.Default.Read(stream, filePath, fileName);
		}
	}
}
