using uTinyRipper.Classes.Misc;

namespace uTinyRipper.BundleFiles
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
