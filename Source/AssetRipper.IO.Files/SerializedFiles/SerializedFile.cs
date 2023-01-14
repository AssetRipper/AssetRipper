using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.Converters;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.IO.Files.Streams.MultiFile;
using AssetRipper.IO.Files.Streams.Smart;
using AssetRipper.IO.Files.Utils;
using System.Collections.Generic;
using System.IO;

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
		public TransferInstructionFlags Flags
		{
			get
			{
				TransferInstructionFlags flags;
				if (SerializedFileMetadata.HasPlatform(Header.Version) && Metadata.TargetPlatform == BuildTarget.NoTarget)
				{
					if (FilePath.EndsWith(".unity", StringComparison.Ordinal))
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

				if (FilenameUtils.IsEngineResource(Name) || (Header.Version < FormatVersion.Unknown_10 && FilenameUtils.IsBuiltinExtra(Name)))
				{
					flags |= TransferInstructionFlags.IsBuiltinResourcesFile;
				}
				if (Header.Endianess || Metadata.SwapEndianess)
				{
					flags |= TransferInstructionFlags.SwapEndianess;
				}
				return flags;
			}
		}
		public EndianType EndianType
		{
			get
			{
				bool swapEndianess = SerializedFileHeader.HasEndianess(Header.Version) ? Header.Endianess : Metadata.SwapEndianess;
				return swapEndianess ? EndianType.BigEndian : EndianType.LittleEndian;
			}
		}

		public IReadOnlyList<FileIdentifier> Dependencies => Metadata.Externals;
		private readonly Dictionary<long, int> m_assetEntryLookup = new();
		public IReadOnlyDictionary<long, int> AssetEntryLookup => m_assetEntryLookup;

		public static bool IsSerializedFile(string filePath)
		{
			using Stream stream = MultiFileStream.OpenRead(filePath);
			return IsSerializedFile(stream);
		}

		public static bool IsSerializedFile(byte[] buffer, int offset, int size)
		{
			using MemoryStream stream = new MemoryStream(buffer, offset, size, false);
			return IsSerializedFile(stream);
		}

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

		public override void Read(SmartStream stream)
		{
			m_assetEntryLookup.Clear();

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

			for (int i = 0; i < Metadata.Object.Length; i++)
			{
				ObjectInfo objectInfo = Metadata.Object[i];
				m_assetEntryLookup.Add(objectInfo.FileID, i);

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
