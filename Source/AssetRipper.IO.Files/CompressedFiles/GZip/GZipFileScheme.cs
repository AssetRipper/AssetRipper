using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files.CompressedFiles.GZip;

public sealed class GZipFileScheme : Scheme<GZipFile>
{
	public override bool CanRead(SmartStream stream)
	{
		using EndianReader reader = new EndianReader(stream, EndianType.BigEndian);
		return GZipFile.IsGZipFile(reader);
	}
}
