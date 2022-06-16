using AssetRipper.Core.Parser.Files.BundleFile.Parser;
using AssetRipper.IO.Endian;

namespace AssetRipper.Core.Parser.Files.BundleFile.Header
{
	public sealed class BundleFileStreamHeader
	{
		public BundleFileStreamHeader(EndianReader reader)
		{
			Size = reader.ReadInt64();
			CompressedBlocksInfoSize = reader.ReadInt32();
			UncompressedBlocksInfoSize = reader.ReadInt32();
			Flags = (BundleFlags)reader.ReadInt32();
		}

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
	}
}
