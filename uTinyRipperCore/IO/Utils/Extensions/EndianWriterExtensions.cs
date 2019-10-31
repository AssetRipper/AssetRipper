using System;

namespace uTinyRipper
{
	public static class EndianWriterExtensions
	{
		public static void Write(this EndianWriter writer, Tuple<int, long> value)
		{
			writer.Write(value.Item1);
			writer.Write(value.Item2);
		}

		public static void Write<T>(this EndianWriter writer, Tuple<T, long> value, Func<T, int> converter)
		{
			writer.Write(converter(value.Item1));
			writer.Write(value.Item2);
		}
	}
}
