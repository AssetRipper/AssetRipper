using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.BundleFiles.FileStream;

public sealed record class BlocksInfo : IEndianReadable<BlocksInfo>, IEndianWritable
{
	public BlocksInfo()
	{
		UncompressedDataHash = new();
		StorageBlocks = Array.Empty<StorageBlock>();
	}

	public BlocksInfo(Hash128 uncompressedDataHash, StorageBlock[] storageBlocks)
	{
		UncompressedDataHash = uncompressedDataHash;
		StorageBlocks = storageBlocks;
	}

	public static BlocksInfo Read(EndianReader reader)
	{
		return new BlocksInfo(reader.ReadEndian<Hash128>(), reader.ReadEndianArray<StorageBlock>());
	}

	public void Write(EndianWriter writer)
	{
		UncompressedDataHash.Write(writer);
		writer.WriteEndianArray(StorageBlocks);
	}

	public Hash128 UncompressedDataHash { get; }
	public StorageBlock[] StorageBlocks { get; set; }
}
