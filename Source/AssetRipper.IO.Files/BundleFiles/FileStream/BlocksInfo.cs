using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.BundleFiles.FileStream
{
	public sealed record class BlocksInfo : IEndianReadable, IEndianWritable
	{
		public void Read(EndianReader reader)
		{
			UncompressedDataHash.Read(reader);
			StorageBlocks = reader.ReadEndianArray<StorageBlock>();
		}

		public void Write(EndianWriter writer)
		{
			UncompressedDataHash.Write(writer);
			writer.WriteEndianArray(StorageBlocks);
		}

		public Hash128 UncompressedDataHash { get; } = new();
		public StorageBlock[] StorageBlocks { get; set; } = Array.Empty<StorageBlock>();
	}
}
