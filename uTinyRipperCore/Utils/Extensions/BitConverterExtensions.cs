using System;

namespace uTinyRipper
{
	public static class BitConverterExtensions
	{
		public static uint ToUInt32(float value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			return BitConverter.ToUInt32(bytes, 0);
		}

		public static ulong ToUInt64(double value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			return BitConverter.ToUInt64(bytes, 0);
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
