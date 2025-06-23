using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files.BundleFiles.Archive;

public sealed class ArchiveBundleScheme : Scheme<ArchiveBundleFile>
{
	public override bool CanRead(SmartStream stream)
	{
		return ArchiveBundleHeader.IsBundleHeader(new EndianReader(stream, EndianType.BigEndian));
	}
}
