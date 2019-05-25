using System.Collections.Generic;
using uTinyRipper.Assembly;

namespace uTinyRipper.SerializedFiles
{
	public sealed class SerializedFileScheme : FileScheme
	{
		private SerializedFileScheme(SmartStream stream, long offset, long size, string filePath, string fileName, TransferInstructionFlags flags) :
			base(stream, offset, size, filePath, fileName)
		{
			Flags = flags;

			Header = new SerializedFileHeader(Name);
			Metadata = new SerializedFileMetadata(Name);
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

		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		private static bool IsMetadataAtTheEnd(FileGeneration generation)
		{
			return generation <= FileGeneration.FG_300_342;
		}
		
		public SerializedFile ReadFile(IFileCollection collection, IAssemblyManager manager)
		{
			SerializedFile file = new SerializedFile(collection, manager, this);
			using (PartialStream stream = new PartialStream(m_stream, m_offset, m_size))
			{
				EndianType endianess = Header.SwapEndianess ? EndianType.BigEndian : EndianType.LittleEndian;
				using (EndianReader reader = new EndianReader(stream, endianess, stream.Position))
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

				EndianType endianess = Header.SwapEndianess ? EndianType.BigEndian : EndianType.LittleEndian;
				using (SerializedFileReader reader = new SerializedFileReader(stream, endianess, Header.Generation))
				{
					if (IsMetadataAtTheEnd(reader.Generation))
					{
						reader.BaseStream.Position = Header.FileSize - Header.MetadataSize;
						reader.BaseStream.Position++;
					}

					Metadata.Read(reader);
				}
			}

#warning TEMP HACK
			Flags = Metadata.Hierarchy.Platform == Platform.NoTarget ? TransferInstructionFlags.NoTransferInstructionFlags : Flags;
			if (FilenameUtils.IsEngineResource(Name) || Header.Generation < FileGeneration.FG_500a1 && FilenameUtils.IsBuiltinExtra(Name))
			{
				Flags |= TransferInstructionFlags.IsBuiltinResourcesFile;
			}
			Flags |= Header.SwapEndianess ? TransferInstructionFlags.SwapEndianess : TransferInstructionFlags.NoTransferInstructionFlags;
		}

		public TransferInstructionFlags Flags { get; private set; }

		public SerializedFileHeader Header { get; private set; }
		public SerializedFileMetadata Metadata { get; private set; }

		public override FileEntryType SchemeType => FileEntryType.Serialized;
		public override IEnumerable<FileIdentifier> Dependencies => Metadata.Dependencies;

		public const TransferInstructionFlags DefaultFlags = TransferInstructionFlags.SerializeGameRelease;
	}
}
