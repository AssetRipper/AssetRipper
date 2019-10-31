using System;

namespace uTinyRipper
{
	public static class AssetWriterExtensions
	{
		public static void Write(this AssetWriter writer, Tuple<int, long> value)
		{
			EndianWriterExtensions.Write(writer, value);
		}

		public static void Write<T>(this AssetWriter writer, Tuple<T, long> value, Func<T, int> converter)
		{
			EndianWriterExtensions.Write(writer, value, converter);
		}
	}
}
