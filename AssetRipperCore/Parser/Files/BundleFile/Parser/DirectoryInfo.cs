using AssetRipper.Core.Parser.Files.BundleFile.IO;

namespace AssetRipper.Core.Parser.Files.BundleFile.Parser
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
