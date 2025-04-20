using System.Buffers;
using System.Text;

namespace AssetRipper.IO.Files;

internal static class Utf8Truncation
{
	public static string TruncateToUTF8ByteLength(string str, int maxLength)
	{
		int currentByteLength = Encoding.UTF8.GetByteCount(str);
		byte[] bytes = ArrayPool<byte>.Shared.Rent(currentByteLength);
		Encoding.UTF8.GetBytes(str, bytes);
		int validLength = FindValidByteLength(bytes.AsSpan(0, currentByteLength), maxLength);
		string result = Encoding.UTF8.GetString(bytes.AsSpan(0, validLength));
		ArrayPool<byte>.Shared.Return(bytes);
		return result;
	}

	private static int FindValidByteLength(ReadOnlySpan<byte> bytes, int maxLength)
	{
		int validLength = maxLength;

		// ascii char:      0_
		// two-byte char:   110_   10_
		// three-byte char: 1110_  10_ _10_
		// four-byte char : 11110_ 10_ _10_ _10

		if (maxLength >= bytes.Length)
		{
			return bytes.Length;
		}

		// next byte is a beginning, so we can safely truncate to maxLength
		byte nextByte = bytes[maxLength];
		if ((nextByte & 0b11_000000) != 0b10_000000)
		{
			return maxLength;
		}

		// move to end of the last full sequence
		for (int i = maxLength - 1; i >= 0; i--)
		{
			byte currentByte = bytes[i];

			if ((currentByte & 0b11_000000) == 0b10_000000)
			{
				// continuation byte
				validLength--;
			}
			else if ((currentByte & 0b10000000) == 0b10000000)
			{
				// start of multi-byte sequence
				validLength--;
				break;
			}
			else
			{
				// ascii char
				break;
			}
		}

		return validLength;
	}
}
