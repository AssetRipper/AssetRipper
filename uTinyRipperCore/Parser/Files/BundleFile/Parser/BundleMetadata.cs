namespace uTinyRipper.BundleFiles
{
	/// <summary>
	/// Metadata about bundle's block or chunk
	/// </summary>
	public sealed class BundleMetadata : IBundleReadable
	{
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		private static bool HasBlocksInfo(BundleType signature) => signature == BundleType.UnityFS;

		public void Read(BundleReader reader)
		{
			if (HasBlocksInfo(reader.Signature))
			{
				BlocksInfo.Read(reader);
				if (reader.Flags.IsBlocksAndDirectoryInfoCombined())
				{
					DirectoryInfo.Read(reader);
				}
			}
			else
			{
				DirectoryInfo.Read(reader);
				reader.AlignStream();
			}
		}

		public BlocksInfo BlocksInfo;
		public DirectoryInfo DirectoryInfo;
	}
}
