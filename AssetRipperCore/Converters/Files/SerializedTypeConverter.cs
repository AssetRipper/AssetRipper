using AssetRipper.Parser.Files.SerializedFiles;
using AssetRipper.Parser.Files.SerializedFiles.Parser;

namespace AssetRipper.Converters.Files
{
	public static class SerializedTypeConverter
	{
		public static void CombineFormats(FormatVersion generation, ref SerializedType origin)
		{
			if (origin.OldType != null)
			{
				TypeTreeConverter.CombineFormats(generation, origin.OldType);
			}
		}
	}
}
