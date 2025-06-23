using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.BundleFiles.RawWeb.Raw;

public sealed record class RawBundleHeader : RawWebBundleHeader
{
	private const string UnityRawMagic = "UnityRaw";
	protected override string MagicString => UnityRawMagic;
	internal static bool IsBundleHeader(EndianReader reader) => IsBundleHeader(reader, UnityRawMagic);
}
