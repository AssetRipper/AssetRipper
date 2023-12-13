namespace AssetRipper.GUI.Web.Pages;

internal static class HexTable
{
	public static void Write(TextWriter writer, byte[] data)
	{
		const int BytesPerRow = 16;

		for (int i = 0; i < data.Length; i += BytesPerRow)
		{
			using (new Tr(writer).End())
			{
				int remainder = data.Length - i;
				int nonEmptyCells = int.Min(remainder, BytesPerRow);
				for (int j = 0; j < nonEmptyCells; j++)
				{
					using (new Td(writer).End())
					{
						//We optimize this because it's a hot path.
						const string HexChars = "0123456789ABCDEF";
						byte value = data[i + j];
						writer.Write(HexChars[value >> 4]);
						writer.Write(HexChars[value & 0xF]);
					}
				}
				int emptyCells = BytesPerRow - remainder;
				for (int k = 0; k < emptyCells; k++)
				{
					new Td(writer).Close();
				}
			}
		}
	}
}
