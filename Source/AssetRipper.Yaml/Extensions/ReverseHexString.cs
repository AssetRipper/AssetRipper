using System.Buffers.Binary;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AssetRipper.Yaml.Extensions;

internal static class ReverseHexString
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetHexStringLength<T>() => Unsafe.SizeOf<T>() * 2;

	public static void WriteReverseHexString<T>(T value, Span<char> buffer) where T : unmanaged
	{
		if (typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte))
		{
			byte b = Unsafe.As<T, byte>(ref value);
			buffer[0] = NybbleToLowercaseHexCharacter(b >> 4);
			buffer[1] = NybbleToLowercaseHexCharacter(b & 0x0F);
		}
		else if (typeof(T) == typeof(short) || typeof(T) == typeof(ushort) || typeof(T) == typeof(char))
		{
			ushort reverse = BinaryPrimitives.ReverseEndianness(Unsafe.As<T, ushort>(ref value));
			buffer[0] = NybbleToLowercaseHexCharacter(reverse >> 12);
			buffer[1] = NybbleToLowercaseHexCharacter((reverse >> 8) & 0x0F);
			buffer[2] = NybbleToLowercaseHexCharacter((reverse >> 4) & 0x0F);
			buffer[3] = NybbleToLowercaseHexCharacter(reverse & 0x0F);
		}
		else if (typeof(T) == typeof(int) || typeof(T) == typeof(uint))
		{
			int reverse = BinaryPrimitives.ReverseEndianness(Unsafe.As<T, int>(ref value));
			buffer[0] = NybbleToLowercaseHexCharacter(reverse >> 28);
			buffer[1] = NybbleToLowercaseHexCharacter((reverse >> 24) & 0x0F);
			buffer[2] = NybbleToLowercaseHexCharacter((reverse >> 20) & 0x0F);
			buffer[3] = NybbleToLowercaseHexCharacter((reverse >> 16) & 0x0F);
			buffer[4] = NybbleToLowercaseHexCharacter((reverse >> 12) & 0x0F);
			buffer[5] = NybbleToLowercaseHexCharacter((reverse >> 8) & 0x0F);
			buffer[6] = NybbleToLowercaseHexCharacter((reverse >> 4) & 0x0F);
			buffer[7] = NybbleToLowercaseHexCharacter(reverse & 0x0F);
		}
		else if (typeof(T) == typeof(long) || typeof(T) == typeof(ulong))
		{
			long reverse = BinaryPrimitives.ReverseEndianness(Unsafe.As<T, long>(ref value));
			buffer[0] = NybbleToLowercaseHexCharacter(reverse >> 60);
			buffer[1] = NybbleToLowercaseHexCharacter((reverse >> 56) & 0x0F);
			buffer[2] = NybbleToLowercaseHexCharacter((reverse >> 52) & 0x0F);
			buffer[3] = NybbleToLowercaseHexCharacter((reverse >> 48) & 0x0F);
			buffer[4] = NybbleToLowercaseHexCharacter((reverse >> 44) & 0x0F);
			buffer[5] = NybbleToLowercaseHexCharacter((reverse >> 40) & 0x0F);
			buffer[6] = NybbleToLowercaseHexCharacter((reverse >> 36) & 0x0F);
			buffer[7] = NybbleToLowercaseHexCharacter((reverse >> 32) & 0x0F);
			buffer[8] = NybbleToLowercaseHexCharacter((reverse >> 28) & 0x0F);
			buffer[9] = NybbleToLowercaseHexCharacter((reverse >> 24) & 0x0F);
			buffer[10] = NybbleToLowercaseHexCharacter((reverse >> 20) & 0x0F);
			buffer[11] = NybbleToLowercaseHexCharacter((reverse >> 16) & 0x0F);
			buffer[12] = NybbleToLowercaseHexCharacter((reverse >> 12) & 0x0F);
			buffer[13] = NybbleToLowercaseHexCharacter((reverse >> 8) & 0x0F);
			buffer[14] = NybbleToLowercaseHexCharacter((reverse >> 4) & 0x0F);
			buffer[15] = NybbleToLowercaseHexCharacter(reverse & 0x0F);
		}
		else if (typeof(T) == typeof(float))
		{
			WriteReverseHexString(BitConverter.SingleToUInt32Bits(Unsafe.As<T, float>(ref value)), buffer);
		}
		else if (typeof(T) == typeof(double))
		{
			WriteReverseHexString(BitConverter.DoubleToUInt64Bits(Unsafe.As<T, double>(ref value)), buffer);
		}
		else
		{
			Debug.Fail($"Unsupported type {typeof(T).FullName} for reverse hex string conversion.");
		}
	}

	private static char NybbleToLowercaseHexCharacter(int x)
	{
		const int Zero = (int)'0';
		const int Offset = (int)'a' - Zero - 0x0A;
		return unchecked((char)(x + Zero + ((9 - x) >> 31 & Offset)));
	}
	private static char NybbleToLowercaseHexCharacter(long x) => NybbleToLowercaseHexCharacter(unchecked((int)x));
}
