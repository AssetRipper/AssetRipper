namespace AssetRipper.SourceGenerated.Extensions;

public static class TextWriterExtensions
{
	public static void WriteString(this TextWriter writer, string @string, int offset, int length)
	{
		writer.Write(@string.AsSpan().Slice(offset, length));
	}

	public static void WriteIndent(this TextWriter writer, int count)
	{
		for (int i = 0; i < count; i++)
		{
			writer.Write('\t');
		}
	}
}
