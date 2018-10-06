using System;
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

		public static uint ToUInt32(float value)
		{
			return new FloatUIntUnion { Float = value }.Int;
		}

		public static ulong ToUInt64(double value)
		{
			return unchecked((ulong)BitConverter.DoubleToInt64Bits(value));
		}

		public static void GetBytes(ushort value, byte[] buffer, int offset)
		{
			buffer[offset + 0] = unchecked((byte)((value & 0xFF) >> 0));
			buffer[offset + 1] = unchecked((byte)((value & 0xFF00) >> 8));
		}

		public static void GetBytes(uint value, byte[] buffer, int offset)
		{
			buffer[offset + 0] = unchecked((byte)((value & 0xFF) >> 0));
			buffer[offset + 1] = unchecked((byte)((value & 0xFF00) >> 8));
			buffer[offset + 2] = unchecked((byte)((value & 0xFF0000) >> 16));
			buffer[offset + 3] = unchecked((byte)((value & 0xFF000000) >> 24));
		}

		public static uint Reverse(uint value)
		{
			uint reverse = (value & 0x000000FF) << 24 | (value & 0x0000FF00) << 8 |
				(value & 0x00FF0000) >> 8 | (value & 0xFF000000) >> 24;
			return reverse;
		}

		public static int Reverse(int value)
		{
			unchecked
			{
				uint uvalue = (uint)value;
				uint reverse = Reverse(uvalue);
				return (int)reverse;
			}
		}

		public static int GetDigitsCount(uint value)
		{
			int count = 0;
			while(value != 0)
			{
				value /= 10;
				count++;
			}
			return count;
		}
	}
}
