using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.BundleFiles.RawWeb.Web;

public sealed record class WebBundleHeader : RawWebBundleHeader
{
	private const string UnityWebMagic = "UnityWeb";
	protected override string MagicString => UnityWebMagic;
	internal static bool IsBundleHeader(EndianReader reader) => IsBundleHeader(reader, UnityWebMagic);
}
