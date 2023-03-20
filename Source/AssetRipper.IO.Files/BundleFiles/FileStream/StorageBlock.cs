using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.BundleFiles.FileStream
{
	/// <summary>
	/// Contains compression information about a block<br/>
	/// Blocks are similar to chunk structure in that it contains a data blob but without file entries
	/// </summary>
	public sealed record class StorageBlock : IEndianReadable<StorageBlock>, IEndianWritable
	{
		public static StorageBlock Read(EndianReader reader)
		{
			return new()
			{
				UncompressedSize = reader.ReadUInt32(),
				CompressedSize = reader.ReadUInt32(),
				Flags = (StorageBlockFlags)reader.ReadUInt16()
			};
		}

		public void Write(EndianWriter writer)
		{
			writer.Write(UncompressedSize);
			writer.Write(CompressedSize);
			writer.Write((ushort)Flags);
		}

		public uint UncompressedSize { get; private set; }
		public uint CompressedSize { get; private set; }
		public StorageBlockFlags Flags { get; private set; }
		public CompressionType CompressionType
		{
			get
			{
				return Flags.GetCompression();
			}
			private set
			{
				Flags = (Flags & ~StorageBlockFlags.CompressionTypeMask) | (StorageBlockFlags.CompressionTypeMask & (StorageBlockFlags)value);
			}
		}
	}
}
