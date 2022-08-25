using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.BundleFiles.FileStream
{
	public sealed record class BlocksInfo : IEndianReadable, IEndianWritable
	{
		public void Read(EndianReader reader)
		{
			Hash.Read(reader);
			StorageBlocks = reader.ReadEndianArray<StorageBlock>();
		}

		public void Write(EndianWriter writer)
		{
			Hash.Write(writer);
			writer.WriteEndianArray(StorageBlocks);
		}

		public Hash128 Hash { get; } = new();
		public StorageBlock[] StorageBlocks { get; set; } = Array.Empty<StorageBlock>();
	}
}
