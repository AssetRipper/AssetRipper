using System.Diagnostics;
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
			byte x = Unsafe.As<T, byte>(ref value);
			buffer[0] = NybbleToLowercaseHexCharacter(x >> 4);
			buffer[1] = NybbleToLowercaseHexCharacter(x & 0x0F);
		}
		else if (typeof(T) == typeof(short) || typeof(T) == typeof(ushort) || typeof(T) == typeof(char))
		{
			ushort x = Unsafe.As<T, ushort>(ref value);
			buffer[0] = NybbleToLowercaseHexCharacter((x >> 4) & 0x0F);
			buffer[1] = NybbleToLowercaseHexCharacter(x & 0x0F);
			buffer[2] = NybbleToLowercaseHexCharacter(x >> 12);
			buffer[3] = NybbleToLowercaseHexCharacter((x >> 8) & 0x0F);
		}
		else if (typeof(T) == typeof(int) || typeof(T) == typeof(uint))
		{
			int x = Unsafe.As<T, int>(ref value);
			buffer[0] = NybbleToLowercaseHexCharacter((x >> 4) & 0x0F);
			buffer[1] = NybbleToLowercaseHexCharacter(x & 0x0F);
			buffer[2] = NybbleToLowercaseHexCharacter((x >> 12) & 0x0F);
			buffer[3] = NybbleToLowercaseHexCharacter((x >> 8) & 0x0F);
			buffer[4] = NybbleToLowercaseHexCharacter((x >> 20) & 0x0F);
			buffer[5] = NybbleToLowercaseHexCharacter((x >> 16) & 0x0F);
			buffer[6] = NybbleToLowercaseHexCharacter(x >> 28);
			buffer[7] = NybbleToLowercaseHexCharacter((x >> 24) & 0x0F);
		}
		else if (typeof(T) == typeof(long) || typeof(T) == typeof(ulong))
		{
			long x = Unsafe.As<T, long>(ref value);
			buffer[0] = NybbleToLowercaseHexCharacter((x >> 4) & 0x0F);
			buffer[1] = NybbleToLowercaseHexCharacter(x & 0x0F);
			buffer[2] = NybbleToLowercaseHexCharacter((x >> 12) & 0x0F);
			buffer[3] = NybbleToLowercaseHexCharacter((x >> 8) & 0x0F);
			buffer[4] = NybbleToLowercaseHexCharacter((x >> 20) & 0x0F);
			buffer[5] = NybbleToLowercaseHexCharacter((x >> 16) & 0x0F);
			buffer[6] = NybbleToLowercaseHexCharacter((x >> 28) & 0x0F);
			buffer[7] = NybbleToLowercaseHexCharacter((x >> 24) & 0x0F);
			buffer[8] = NybbleToLowercaseHexCharacter((x >> 36) & 0x0F);
			buffer[9] = NybbleToLowercaseHexCharacter((x >> 32) & 0x0F);
			buffer[10] = NybbleToLowercaseHexCharacter((x >> 44) & 0x0F);
			buffer[11] = NybbleToLowercaseHexCharacter((x >> 40) & 0x0F);
			buffer[12] = NybbleToLowercaseHexCharacter((x >> 52) & 0x0F);
			buffer[13] = NybbleToLowercaseHexCharacter((x >> 48) & 0x0F);
			buffer[14] = NybbleToLowercaseHexCharacter(x >> 60);
			buffer[15] = NybbleToLowercaseHexCharacter((x >> 56) & 0x0F);
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
