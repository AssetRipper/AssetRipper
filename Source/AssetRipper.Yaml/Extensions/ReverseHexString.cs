using System.Buffers.Binary;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AssetRipper.Yaml.Extensions;

internal static class ReverseHexString
{
	public static int GetHexStringLength<T>() => Unsafe.SizeOf<T>() * 2;

	public static int WriteReverseHexString<T>(T value, Span<char> buffer) where T : INumber<T>
	{
		int charactersWritten;
		if (typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte))
		{
			value.TryFormat(buffer, out charactersWritten, "x2", CultureInfo.InvariantCulture);
		}
		else if (typeof(T) == typeof(short) || typeof(T) == typeof(ushort) || typeof(T) == typeof(char))
		{
			ushort reverse = BinaryPrimitives.ReverseEndianness(Unsafe.As<T, ushort>(ref value));
			reverse.TryFormat(buffer, out charactersWritten, "x4", CultureInfo.InvariantCulture);
		}
		else if (typeof(T) == typeof(int) || typeof(T) == typeof(uint))
		{
			uint reverse = BinaryPrimitives.ReverseEndianness(Unsafe.As<T, uint>(ref value));
			reverse.TryFormat(buffer, out charactersWritten, "x8", CultureInfo.InvariantCulture);
		}
		else if (typeof(T) == typeof(long) || typeof(T) == typeof(ulong))
		{
			ulong reverse = BinaryPrimitives.ReverseEndianness(Unsafe.As<T, ulong>(ref value));
			reverse.TryFormat(buffer, out charactersWritten, "x16", CultureInfo.InvariantCulture);
		}
		else if (typeof(T) == typeof(float))
		{
			return WriteReverseHexString(BitConverter.SingleToUInt32Bits(Unsafe.As<T, float>(ref value)), buffer);
		}
		else if (typeof(T) == typeof(double))
		{
			return WriteReverseHexString(BitConverter.DoubleToUInt64Bits(Unsafe.As<T, double>(ref value)), buffer);
		}
		else
		{
			charactersWritten = 0;
		}
		return charactersWritten;
	}
}
