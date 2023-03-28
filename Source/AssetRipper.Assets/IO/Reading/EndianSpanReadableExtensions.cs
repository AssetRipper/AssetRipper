using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Assets.IO.Reading;

public static class EndianSpanReadableExtensions
{
	public static void Read(this IEndianSpanReadable asset, ref EndianSpanReader reader, TransferInstructionFlags flags)
	{
		if (flags.IsRelease())
		{
			asset.ReadRelease(ref reader);
		}
		else
		{
			asset.ReadEditor(ref reader);
		}
	}
}
