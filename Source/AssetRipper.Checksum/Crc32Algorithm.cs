using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Hashing;
using System.Text;

namespace AssetRipper.Checksum;

public static partial class Crc32Algorithm
{
	[ThreadStatic]
	private static Crc32? crc;

	public static uint HashData(ReadOnlySpan<byte> data)
	{
		Span<byte> buffer = stackalloc byte[sizeof(uint)];
		Crc32.Hash(data, buffer);
		return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
	}

	public static uint HashAscii(ReadOnlySpan<char> data)
	{
		crc ??= new();
		crc.Reset();
		crc.UpdateAscii(data);

		return crc.FinishHash();
	}

	private static uint HashAscii(ReadOnlySpan<char> str1, ReadOnlySpan<char> str2)
	{
		crc ??= new();
		crc.Reset();
		crc.UpdateAscii(str1);
		crc.UpdateAscii(str2);

		return crc.FinishHash();
	}

	public static uint HashUTF8(string data)
	{
		return HashUTF8(data.AsSpan());
	}

	public static uint HashUTF8(ReadOnlySpan<char> span)
	{
		int count = (int)(uint)Encoding.UTF8.GetByteCount(span);

		byte[]? rentedArray;
		scoped Span<byte> buffer;
		if (count > 1024)
		{
			rentedArray = ArrayPool<byte>.Shared.Rent(count);
			buffer = new Span<byte>(rentedArray, 0, count);
		}
		else
		{
			rentedArray = null;
			buffer = stackalloc byte[count];
		}

		int encoded = Encoding.UTF8.GetBytes(span, buffer);
		Debug.Assert(encoded == count);

		uint result = HashData(buffer);

		if (rentedArray is not null)
		{
			ArrayPool<byte>.Shared.Return(rentedArray);
		}

		return result;
	}

	public static bool MatchAscii(ReadOnlySpan<char> data, uint hash)
	{
		return HashAscii(data) == hash;
	}

	public static bool MatchUTF8(string data, uint hash)
	{
		return HashUTF8(data) == hash;
	}

	private static void UpdateAscii(this Crc32 @this, ReadOnlySpan<char> data)
	{
		for (int i = 0; i < data.Length; i++)
		{
			byte b = (byte)data[i];
			@this.Append(new ReadOnlySpan<byte>(in b));
		}
	}

	private static uint FinishHash(this Crc32 @this)
	{
		Span<byte> buffer = stackalloc byte[sizeof(uint)];
		@this.GetHashAndReset(buffer);
		return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
	}
}
