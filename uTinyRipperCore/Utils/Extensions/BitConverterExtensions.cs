using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace uTinyRipper
{
	public static class BitConverterExtensions
	{
		[StructLayout(LayoutKind.Explicit)]
		private struct FloatUIntUnion
		{
			[FieldOffset(0)]
			public uint Int;
			[FieldOffset(0)]
			public float Float;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint ToUInt32(float value)
		{
			return new FloatUIntUnion { Float = value }.Int;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong ToUInt64(double value)
		{
			return unchecked((ulong)BitConverter.DoubleToInt64Bits(value));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ToSingle(uint value)
		{
			return new FloatUIntUnion { Int = value }.Float;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double ToDouble(ulong value)
		{
			return BitConverter.Int64BitsToDouble(unchecked((long)value));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetBytes(ushort value, byte[] buffer, int offset)
		{
			buffer[offset + 0] = unchecked((byte)(value >> 0));
			buffer[offset + 1] = unchecked((byte)(value >> 8));
		}

		public static void GetBytes(uint value, byte[] buffer, int offset)
		{
			buffer[offset + 0] = unchecked((byte)(value >> 0));
			buffer[offset + 1] = unchecked((byte)(value >> 8));
			buffer[offset + 2] = unchecked((byte)(value >> 16));
			buffer[offset + 3] = unchecked((byte)(value >> 24));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static short Swap(short value)
		{
			return unchecked((short)(Swap(unchecked((ushort)value))));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort Swap(ushort value)
		{
			return unchecked((ushort)(value >> 8 | value << 8));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Swap(int value)
		{
			return unchecked((int)(Swap(unchecked((uint)value))));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Swap(uint value)
		{
			value = value >> 16 | value << 16;
			return ((value & 0xFF00FF00) >> 8) | ((value & 0x00FF00FF) << 8);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Swap(long value)
		{
			return unchecked((long)(Swap(unchecked((ulong)value))));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong Swap(ulong value)
		{
			value = value >> 32 | value << 32;
			value = ((value & 0xFFFF0000FFFF0000) >> 16) | ((value & 0x0000FFFF0000FFFF) << 16);
			return ((value & 0xFF00FF00FF00FF00) >> 8) | ((value & 0x00FF00FF00FF00FF) << 8);
		}

		public static int GetDigitsCount(uint value)
		{
			int count = 0;
			while (value != 0)
			{
				value /= 10;
				count++;
			}
			return count;
		}
	}
}
