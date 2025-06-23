using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files.BundleFiles.RawWeb.Web;

public sealed class WebBundleScheme : Scheme<WebBundleFile>
{
	public override bool CanRead(SmartStream stream)
	{
		return WebBundleHeader.IsBundleHeader(new EndianReader(stream, EndianType.BigEndian));
	}
}
