using AssetRipper.Parser.Classes.Misc;
using AssetRipper.Parser.Files.BundleFile.IO;

namespace AssetRipper.Parser.Files.BundleFile.Parser
{
	public struct BlocksInfo : IBundleReadable
	{
		public void Read(BundleReader reader)
		{
			Hash.Read(reader);
			StorageBlocks = reader.ReadBundleArray<StorageBlock>();
		}

		public StorageBlock[] StorageBlocks { get; set; }

		public Hash128 Hash;
	}
}
