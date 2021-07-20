using AssetRipper.Parser.Files.SerializedFile;
using AssetRipper.Parser.Files.SerializedFile.Parser;

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
