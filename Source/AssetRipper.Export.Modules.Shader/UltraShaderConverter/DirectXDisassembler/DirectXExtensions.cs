namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.DirectXDisassembler;

public static class DirectXExtensions
{
	public static long Align(this BinaryReader reader, int bytes)
	{
		long position = reader.BaseStream.Position;
		return position + (bytes - position % bytes) % bytes;
	}
	public static string ReadNullString(this BinaryReader reader)
	{
		char character;
		string result = "";
		while ((character = reader.ReadChar()) != 0 && reader.BaseStream.Position < reader.BaseStream.Length)
		{
			result += character;
		}
		return result;
	}
}
