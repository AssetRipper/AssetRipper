using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.IO.Files.Converters
{
	public static class SerializedTypeConverter
	{
		public static void CombineFormats(FormatVersion generation, SerializedType origin)
		{
			origin.OldType.MaybeSetNamesFromBuffer();
		}
	}
}
