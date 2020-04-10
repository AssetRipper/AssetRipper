namespace uTinyRipper.BundleFiles
{
	public struct DirectoryInfo : IBundleReadable
	{
		public void Read(BundleReader reader)
		{
			Nodes = reader.ReadBundleArray<Node>();
		}

		public Node[] Nodes { get; set; }
	}
}
