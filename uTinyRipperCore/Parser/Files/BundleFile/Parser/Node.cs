namespace uTinyRipper.BundleFiles
{
	public sealed class Node : IBundleReadable
	{
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		public static bool HasBlobIndex(BundleType signature) => signature == BundleType.UnityFS;

		public void Read(BundleReader reader)
		{
			if (HasBlobIndex(reader.Signature))
			{
				Offset = reader.ReadInt64();
				Size = reader.ReadInt64();
				BlobIndex = reader.ReadInt32();
				PathOrigin = reader.ReadStringZeroTerm();
			}
			else
			{
				PathOrigin = reader.ReadStringZeroTerm();
				Offset = reader.ReadInt32();
				Size = reader.ReadInt32();
			}
			Path = FilenameUtils.FixFileIdentifier(PathOrigin);
		}

		public override string ToString()
		{
			return Path;
		}

		public string Path { get; set; }
		public string PathOrigin { get; set; }
		public long Offset { get; set; }
		public long Size { get; set; }
		public int BlobIndex { get; set; }
	}
}
