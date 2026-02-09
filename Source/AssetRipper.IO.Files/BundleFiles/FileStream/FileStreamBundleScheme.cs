using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files.BundleFiles.FileStream;

public sealed class FileStreamBundleScheme : Scheme<FileStreamBundleFile>
{
	public override bool CanRead(SmartStream stream)
	{
		return FileStreamBundleHeader.IsBundleHeader(new EndianReader(stream, EndianType.BigEndian));
	}
}
