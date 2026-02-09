using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files.BundleFiles.RawWeb.Raw;

public sealed class RawBundleScheme : Scheme<RawBundleFile>
{
	public override bool CanRead(SmartStream stream)
	{
		return RawBundleHeader.IsBundleHeader(new EndianReader(stream, EndianType.BigEndian));
	}
}
