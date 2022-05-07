using System.IO;

namespace AssetRipper.Core.Extensions
{
	public static class TextWriterExtensions
	{
		public static void WriteString(this TextWriter writer, string @string, int offset, int length)
		{
			for (int i = offset; i < offset + length; i++)
			{
				writer.Write(@string[i]);
			}
		}

		public static void WriteIndent(this TextWriter writer, int count)
		{
			for (int i = 0; i < count; i++)
			{
				writer.Write('\t');
			}
		}
	}
}
