using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files.CompressedFiles.Brotli;

public sealed class BrotliFileScheme : Scheme<BrotliFile>
{
	public override bool CanRead(SmartStream stream)
	{
		return BrotliFile.IsBrotliFile(stream);
	}
}
