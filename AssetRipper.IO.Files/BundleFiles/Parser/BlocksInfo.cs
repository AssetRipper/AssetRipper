using AssetRipper.IO.Files.BundleFiles.IO;

namespace AssetRipper.IO.Files.BundleFiles.Parser
{
	public sealed class BlocksInfo : IBundleReadable
	{
		public void Read(BundleReader reader)
		{
			Hash = reader.ReadBytes(16);
			StorageBlocks = reader.ReadBundleArray<StorageBlock>();
		}

		public StorageBlock[] StorageBlocks { get; set; } = Array.Empty<StorageBlock>();
		/// <summary>
		/// Hash128
		/// </summary>
		public byte[] Hash { get; set; } = Array.Empty<byte>();
	}
}
