using AssetRipper.Core.Parser.Files.BundleFile.IO;

namespace AssetRipper.Core.Parser.Files.BundleFile.Parser
{
	public sealed class DirectoryInfo : IBundleReadable
	{
		public void Read(BundleReader reader)
		{
			Nodes = reader.ReadBundleArray<Node>();
		}

		public Node[] Nodes { get; set; } = Array.Empty<Node>();
	}
}
