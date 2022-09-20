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
		public TransferInstructionFlags Flags { get; private set; }
		public EndianType EndianType
		{
			get
			{
				bool swapEndianess = SerializedFileHeader.HasEndianess(Header.Version) ? Header.Endianess : Metadata.SwapEndianess;
				return swapEndianess ? EndianType.BigEndian : EndianType.LittleEndian;
			}
		}
		public SmartStream Stream { get; private set; } = SmartStream.Null;

		public IReadOnlyList<FileIdentifier> Dependencies => Metadata.Externals;
		private readonly Dictionary<long, int> m_assetEntryLookup = new();
		public IReadOnlyDictionary<long, int> AssetEntryLookup => m_assetEntryLookup;

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

		public override void Read(SmartStream stream)
		{
			m_assetEntryLookup.Clear();

			Stream = stream;

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
				m_assetEntryLookup.Add(Metadata.Object[i].FileID, i);
			}

			UpdateFlags();
		}

		private void UpdateFlags()
		{
			Flags = TransferInstructionFlags.SerializeGameRelease;
			if (SerializedFileMetadata.HasPlatform(Header.Version))
			{
				if (Metadata.TargetPlatform == BuildTarget.NoTarget)
				{
					Flags = TransferInstructionFlags.NoTransferInstructionFlags;
					if (FilePath.EndsWith(".unity", StringComparison.Ordinal))
					{
						Flags |= TransferInstructionFlags.SerializeEditorMinimalScene;
					}
				}
			}

			if (FilenameUtils.IsEngineResource(Name) || (Header.Version < FormatVersion.Unknown_10 && FilenameUtils.IsBuiltinExtra(Name)))
			{
				Flags |= TransferInstructionFlags.IsBuiltinResourcesFile;
			}
			if (Header.Endianess || Metadata.SwapEndianess)
			{
				Flags |= TransferInstructionFlags.SwapEndianess;
			}
		}

		public override void Write(Stream stream)
		{
			throw new NotImplementedException();
		}
	}
}
