using AssetRipper.IO.Endian;
using System.Diagnostics;

namespace AssetRipper.IO.Files.BundleFiles.FileStream;

public sealed record class FileStreamBundleHeader : BundleHeader
{
	private const string UnityFSMagic = "UnityFS";
	protected override string MagicString => UnityFSMagic;
	/// <summary>
	/// Equal to file size, sometimes equal to uncompressed data size without the header
	/// </summary>
	public long Size { get; set; }
	/// <summary>
	/// UnityFS length of the possibly-compressed (LZMA, LZ4) bundle data header
	/// </summary>
	public int CompressedBlocksInfoSize { get; set; }
	public int UncompressedBlocksInfoSize { get; set; }
	public BundleFlags Flags { get; set; }

	public CompressionType CompressionType
	{
		get
		{
			return Flags.GetCompression();
		}
		set
		{
			Flags = (Flags & ~BundleFlags.CompressionTypeMask) | (BundleFlags.CompressionTypeMask & (BundleFlags)value);
		}
	}

	public override void Read(EndianReader reader)
	{
		base.Read(reader);
		Size = reader.ReadInt64();
		Debug.Assert(Size >= 0);
		CompressedBlocksInfoSize = reader.ReadInt32();
		Debug.Assert(CompressedBlocksInfoSize >= 0);
		UncompressedBlocksInfoSize = reader.ReadInt32();
		Debug.Assert(UncompressedBlocksInfoSize >= 0);
		Flags = (BundleFlags)reader.ReadInt32();
	}

	public override void Write(EndianWriter writer)
	{
		base.Write(writer);
		writer.Write(Size);
		writer.Write(CompressedBlocksInfoSize);
		writer.Write(UncompressedBlocksInfoSize);
		writer.Write((int)Flags);
	}

	internal static bool IsBundleHeader(EndianReader reader) => IsBundleHeader(reader, UnityFSMagic);
}
