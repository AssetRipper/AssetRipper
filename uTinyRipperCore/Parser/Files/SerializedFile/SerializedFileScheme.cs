using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.Converters;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper
{
	public sealed class SerializedFileScheme : FileScheme
	{
		private SerializedFileScheme(byte[] buffer, string filePath, string fileName) :
			base(filePath, fileName)
		{
			Stream = new MemoryStream(buffer, 0, buffer.Length, false);
		}

		private SerializedFileScheme(SmartStream stream, string filePath, string fileName) :
			base(filePath, fileName)
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

		internal SerializedFile ReadFile(GameProcessorContext context)
		{
			SerializedFile file = new SerializedFile(context.Collection, this);
			context.AddSerializedFile(file, this);
			return file;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (Stream != null)
			{
				Stream.Dispose();
				Stream = null;
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
			UpdateFlags();
		}

		private void UpdateFlags()
		{
			Flags = TransferInstructionFlags.SerializeGameRelease;
			if (SerializedFileMetadata.HasPlatform(Header.Version))
			{
				if (Metadata.TargetPlatform == Platform.NoTarget)
				{
					Flags = TransferInstructionFlags.NoTransferInstructionFlags;
					if (FilePath.EndsWith(".unity", StringComparison.Ordinal))
					{
						Flags |= TransferInstructionFlags.SerializeEditorMinimalScene;
					}
				}
			}

			if (FilenameUtils.IsEngineResource(Name) || Header.Version < FormatVersion.Unknown_10 && FilenameUtils.IsBuiltinExtra(Name))
			{
				Flags |= TransferInstructionFlags.IsBuiltinResourcesFile;
			}
			if (Header.Endianess || Metadata.SwapEndianess)
			{
				Flags |= TransferInstructionFlags.SwapEndianess;
			}
		}

		public override FileEntryType SchemeType => FileEntryType.Serialized;
		public override IEnumerable<FileIdentifier> Dependencies => Metadata.Externals;

		public SerializedFileHeader Header { get; } = new SerializedFileHeader();
		public SerializedFileMetadata Metadata { get; } = new SerializedFileMetadata();
		public TransferInstructionFlags Flags { get; set; }

		public Stream Stream { get; private set; }
	}
}
