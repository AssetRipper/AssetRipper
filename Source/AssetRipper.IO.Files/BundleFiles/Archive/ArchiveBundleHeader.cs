using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.BundleFiles.Archive;

public sealed record class ArchiveBundleHeader : BundleHeader
{
	private const string UnityArchiveMagic = "UnityArchive";
	protected override string MagicString => UnityArchiveMagic;
	internal static bool IsBundleHeader(EndianReader reader) => IsBundleHeader(reader, UnityArchiveMagic);
}
