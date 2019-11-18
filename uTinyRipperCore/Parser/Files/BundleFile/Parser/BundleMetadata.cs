namespace uTinyRipper.BundleFiles
{
	/// <summary>
	/// Metadata about bundle's block or chunk
	/// </summary>
	public sealed class BundleMetadata : IBundleReadable
	{
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		private static bool HasBlockInfo(BundleGeneration generation) => generation >= BundleGeneration.BF_530_x;

		public void Read(BundleReader reader)
		{
			if (HasBlockInfo(reader.Generation))
			{
				Unknown0 = reader.ReadInt32();
				Unknown1 = reader.ReadInt32();
				Unknown2 = reader.ReadInt32();
				Unknown3 = reader.ReadInt32();
				BlockInfos = reader.ReadBundleArray<BlockInfo>();
			}

			Entries = reader.ReadBundleArray<BundleFileEntry>();
		}

		public int Unknown0 { get; set; }
		public int Unknown1 { get; set; }
		public int Unknown2 { get; set; }
		public int Unknown3 { get; set; }
		public BlockInfo[] BlockInfos { get; set; }
		public BundleFileEntry[] Entries { get; set; }
	}
}
