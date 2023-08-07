using ICSharpCode.SharpZipLib.Checksum;
using System.Text;

namespace AssetRipper.Checksum;

public static class Crc28Algorithm
{
	private const int Mask = 0xFFFFFFF;

	[ThreadStatic]
	private static Crc32? crc;

	public static uint HashData(byte[] data)
	{
		crc ??= new();
		crc.Reset();
		crc.Update(data);
		return (uint)crc.Value & Mask;
	}

	public static uint HashAscii(ReadOnlySpan<char> data)
	{
		crc ??= new();
		crc.Reset();
		for (int i = 0; i < data.Length; i++)
		{
			byte b = (byte)data[i];
			crc.Update(b);
		}

		return (uint)crc.Value & Mask;
	}

	public static uint HashUTF8(string data)
	{
		return HashData(Encoding.UTF8.GetBytes(data));
	}

	public static bool MatchAscii(ReadOnlySpan<char> data, uint hash)
	{
		return HashAscii(data) == hash;
	}

	public static bool MatchUTF8(string data, uint hash)
	{
		return HashUTF8(data) == hash;
	}
}
