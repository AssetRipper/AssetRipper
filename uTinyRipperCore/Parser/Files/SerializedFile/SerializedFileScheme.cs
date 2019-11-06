using System;
using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.Game;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper
{
	public sealed class SerializedFileScheme : FileScheme
	{
		private SerializedFileScheme(SmartStream stream, long offset, long size, string filePath, string fileName, TransferInstructionFlags flags) :
			base(stream, offset, size, filePath, fileName)
		{
			Flags = flags;

			Header = new SerializedFileHeader();
			Metadata = new SerializedFileMetadata();
		}

		internal static SerializedFileScheme ReadSceme(SmartStream stream, long offset, long size, string filePath, string fileName)
		{
			return ReadSceme(stream, offset, size, filePath, fileName, DefaultFlags);
		}

		internal static SerializedFileScheme ReadSceme(SmartStream stream, long offset, long size, string filePath, string fileName, TransferInstructionFlags flags)
		{
			SerializedFileScheme scheme = new SerializedFileScheme(stream, offset, size, filePath, fileName, flags);
			scheme.ReadScheme();
			return scheme;
		}

		public SerializedFile ReadFile(IFileCollection collection, IAssemblyManager manager)
		{
			SerializedFile file = new SerializedFile(collection, manager, this);
			using (PartialStream stream = new PartialStream(m_stream, m_offset, m_size))
			{
				EndianType endianess = Header.SwapEndianess ? EndianType.BigEndian : EndianType.LittleEndian;
				using (EndianReader reader = new EndianReader(stream, endianess))
				{
					file.Read(reader);
				}
			}
			return file;
		}

		public override bool ContainsFile(string fileName)
		{
			return false;
		}

		private void ReadScheme()
		{
			using (PartialStream stream = new PartialStream(m_stream, m_offset, m_size))
			{
				using (EndianReader reader = new EndianReader(stream, EndianType.BigEndian))
				{
					Header.Read(reader);
				}

				EndianType endianess = EndianType.LittleEndian;
				if (SerializedFileHeader.HasEndian(Header.Generation))
				{
					endianess = Header.SwapEndianess ? EndianType.BigEndian : EndianType.LittleEndian;
				}
				using (SerializedReader reader = new SerializedReader(stream, endianess, Name, Header.Generation))
				{
					if (SerializedFileMetadata.IsMetadataAtTheEnd(reader.Generation))
					{
						reader.BaseStream.Position = Header.FileSize - Header.MetadataSize;
						reader.BaseStream.Position++;
					}

					Metadata.Read(reader);
				}
			}

			SerializedFileMetadataConverter.CombineFormats(Header.Generation, Metadata);
			RTTIClassHierarchyDescriptorConverter.FixResourceVersion(Name, ref Metadata.Hierarchy);

#warning TEMP HACK
			if (Metadata.Hierarchy.Platform == Platform.NoTarget)
			{
				Flags = TransferInstructionFlags.NoTransferInstructionFlags;
				if (FilePath.EndsWith(".unity", StringComparison.Ordinal))
				{
					Flags |= TransferInstructionFlags.SerializeEditorMinimalScene;
				}
			}
			if (FilenameUtils.IsEngineResource(Name) || Header.Generation < FileGeneration.FG_500a1 && FilenameUtils.IsBuiltinExtra(Name))
			{
				Flags |= TransferInstructionFlags.IsBuiltinResourcesFile;
			}
			if (Header.SwapEndianess)
			{
				Flags |= TransferInstructionFlags.SwapEndianess;
			}
		}

		public override FileEntryType SchemeType => FileEntryType.Serialized;
		public override IEnumerable<FileIdentifier> Dependencies => Metadata.Dependencies;

		public SerializedFileHeader Header { get; }
		public SerializedFileMetadata Metadata { get; }
		public TransferInstructionFlags Flags { get; private set; }

		public const TransferInstructionFlags DefaultFlags = TransferInstructionFlags.SerializeGameRelease;
	}
}
