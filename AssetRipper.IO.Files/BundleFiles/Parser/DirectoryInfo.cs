using AssetRipper.IO.Files.BundleFiles.IO;

namespace AssetRipper.IO.Files.BundleFiles.Parser
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
