using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;

namespace AssetRipper.Core.Parser.Files.SerializedFiles
{
	public static class SerializedTypeConverter
	{
		public static void CombineFormats(FormatVersion generation, SerializedType origin)
		{
			origin.OldType.MaybeSetNamesFromBuffer();
		}
	}
}
