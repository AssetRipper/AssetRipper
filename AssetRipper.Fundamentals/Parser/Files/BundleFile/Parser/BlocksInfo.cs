using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Parser.Files.BundleFile.IO;

namespace AssetRipper.Core.Parser.Files.BundleFile.Parser
{
	public sealed class BlocksInfo : IBundleReadable
	{
		public void Read(BundleReader reader)
		{
			Hash.Read(reader);
			StorageBlocks = reader.ReadBundleArray<StorageBlock>();
		}

		public StorageBlock[] StorageBlocks { get; set; } = Array.Empty<StorageBlock>();

		public Hash128 Hash { get; } = new();
	}
}
