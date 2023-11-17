using System.IO.Hashing;
using System.Text;

namespace AssetRipper.Checksum;

public static class Crc28Algorithm
{
	private const int Mask = 0xFFFFFFF;

	public static uint HashData(byte[] data)
	{
		return Crc32Algorithm.HashData(data) & Mask;
	}

	public static uint HashAscii(ReadOnlySpan<char> data)
	{
		return Crc32Algorithm.HashAscii(data) & Mask;
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
