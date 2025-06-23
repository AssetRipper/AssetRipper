using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Assets.IO.Writing;

public interface IAssetWritable
{
	void WriteEditor(AssetWriter writer);
	void WriteRelease(AssetWriter writer);
}
public static class AssetWritableExtensions
{
	public static void Write(this IAssetWritable asset, AssetWriter writer)
	{
		asset.Write(writer, writer.AssetCollection.Flags);
	}

	public static void Write(this IAssetWritable asset, AssetWriter writer, TransferInstructionFlags flags)
	{
		if (flags.IsRelease())
		{
			asset.WriteRelease(writer);
		}
		else
		{
			asset.WriteEditor(writer);
		}
	}
}
