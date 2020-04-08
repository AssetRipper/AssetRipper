using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Converters
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
