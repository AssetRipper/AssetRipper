using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;

namespace AssetRipper.Core.Converters.Files
{
	public static class SerializedTypeConverter
	{
		public static void CombineFormats(FormatVersion generation, ref SerializedType origin)
		{
			if (origin.OldType != null)
			{
				origin.OldType.MaybeSetNamesFromBuffer();
			}
		}
	}
}
