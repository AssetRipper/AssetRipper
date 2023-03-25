using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Assets.IO.Reading
{
	public interface IAssetReadable
	{
		void ReadEditor(AssetReader reader);
		void ReadRelease(AssetReader reader);
	}
	public static class AssetReadableExtensions
	{
		public static void Read(this IAssetReadable asset, AssetReader reader)
		{
			asset.Read(reader, reader.AssetCollection.Flags);
		}

		public static void Read(this IAssetReadable asset, AssetReader reader, TransferInstructionFlags flags)
		{
			if (flags.IsRelease())
			{
				asset.ReadRelease(reader);
			}
			else
			{
				asset.ReadEditor(reader);
			}
		}
	}
}
