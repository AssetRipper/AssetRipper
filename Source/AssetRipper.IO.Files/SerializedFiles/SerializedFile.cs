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
		private FileIdentifier[]? m_dependencies;
		private ObjectInfo[]? m_objects;
		private SerializedType[]? m_types;
		private SerializedTypeReference[]? m_refTypes;

		public FormatVersion Generation { get; private set; }
		public UnityVersion Version { get; private set; }
		public BuildTarget Platform { get; private set; }
		public TransferInstructionFlags Flags { get; private set; }
		public EndianType EndianType { get; private set; }
		public ReadOnlySpan<FileIdentifier> Dependencies => m_dependencies;
		public ReadOnlySpan<ObjectInfo> Objects => m_objects;
		public ReadOnlySpan<SerializedType> Types => m_types;
		public ReadOnlySpan<SerializedTypeReference> RefTypes => m_refTypes;
		public bool HasTypeTree { get; private set; }

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
			SerializedFileHeader header = new();
			using (EndianReader reader = new EndianReader(stream, EndianType.BigEndian))
			{
				header.Read(reader);
			}
			if (SerializedFileMetadata.IsMetadataAtTheEnd(header.Version))
			{
				stream.Position = header.FileSize - header.MetadataSize;
			}
			SerializedFileMetadata metadata = new();
			metadata.Read(stream, header);

			SerializedFileMetadataConverter.CombineFormats(header.Version, metadata);

			foreach (ObjectInfo objectInfo in metadata.Object)
			{
				stream.Position = header.DataOffset + objectInfo.ByteStart;
				byte[] objectData = new byte[objectInfo.ByteSize];
				stream.ReadExactly(objectData);
				objectInfo.ObjectData = objectData;
			}

			SetProperties(header, metadata);
		}

		private void SetProperties(SerializedFileHeader header, SerializedFileMetadata metadata)
		{
			Generation = header.Version;
			Version = metadata.UnityVersion;
			Platform = metadata.TargetPlatform;
			Flags = GetFlags(header, metadata, Name, FilePath);
			EndianType = GetEndianType(header, metadata);
			m_dependencies = metadata.Externals;
			m_objects = metadata.Object;
			m_types = metadata.Types;
			m_refTypes = metadata.RefTypes;
			HasTypeTree = metadata.EnableTypeTree;
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
