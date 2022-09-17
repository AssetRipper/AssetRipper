using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files.BundleFiles.Archive
{
	public sealed class ArchiveBundleFile : File
	{
		public ArchiveBundleHeader Header { get; } = new();

		public override void Read(SmartStream stream)
		{
			throw new NotSupportedException();
		}

		public override void Write(System.IO.Stream stream)
		{
			throw new NotSupportedException();
		}
	}
}
