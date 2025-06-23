using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.BundleFiles.RawWeb;

/// <summary>
/// Contains compression information about chunk
/// Chunk is a structure (optionaly LZMA compressed) that contains file entries and data blob
/// </summary>
public sealed record class BundleScene : IEndianReadable<BundleScene>, IEndianWritable
{
	public static BundleScene Read(EndianReader reader)
	{
		return new()
		{
			CompressedSize = reader.ReadUInt32(),
			DecompressedSize = reader.ReadUInt32()
		};
	}

	public void Write(EndianWriter writer)
	{
		writer.Write(CompressedSize);
		writer.Write(DecompressedSize);
	}

	public uint CompressedSize { get; private set; }
	public uint DecompressedSize { get; private set; }
}
