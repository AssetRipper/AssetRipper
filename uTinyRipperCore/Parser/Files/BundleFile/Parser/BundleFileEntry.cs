namespace uTinyRipper.BundleFiles
{
	public sealed class BundleFileEntry : FileEntry, IBundleFileReadable
	{
		public static bool IsReadBlobIndex(BundleGeneration generation)
		{
			return generation >= BundleGeneration.BF_530_x;
		}

		public void Read(BundleFileReader reader)
		{
			if (IsReadBlobIndex(reader.Generation))
			{
				Offset = reader.ReadInt64();
				Size = reader.ReadInt64();
				BlobIndex = reader.ReadInt32();
				NameOrigin = reader.ReadStringZeroTerm();
			}
			else
			{
				NameOrigin = reader.ReadStringZeroTerm();
				Offset = reader.ReadInt32();
				Size = reader.ReadInt32();
			}
			Name = FilenameUtils.FixFileIdentifier(NameOrigin);
		}

		public int BlobIndex { get; private set; }
	}
}
