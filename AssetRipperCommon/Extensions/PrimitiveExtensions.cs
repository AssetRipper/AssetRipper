using System;

namespace AssetRipper.Core.Extensions
{
	public static class PrimitiveExtensions
	{
		public static int ParseDigit(this char _this)
		{
			return _this - '0';
		}

		public static string ToHexString(this byte _this)
		{
			return _this.ToString("x2");
		}

		public static string ToHexString(this sbyte _this)
		{
			byte value = unchecked((byte)_this);
			return ToHexString(value);
		}

		public static string ToHexString(this short _this)
		{
			ushort value = unchecked((ushort)_this);
			return ToHexString(value);
		}

		public static string ToHexString(this ushort _this)
		{
			ushort reverse = unchecked((ushort)(((0xFF00 & _this) >> 8) | ((0x00FF & _this) << 8)));
			return reverse.ToString("x4");
		}

		public static string ToHexString(this int _this)
		{
			uint value = unchecked((uint)_this);
			return ToHexString(value);
		}

		public static string ToHexString(this uint _this)
		{
			uint reverse = ((0xFF000000 & _this) >> 24) | ((0x00FF0000 & _this) >> 8) | ((0x0000FF00 & _this) << 8) | ((0x000000FF & _this) << 24);
			return reverse.ToString("x8");
		}

		public static string ToHexString(this long _this)
		{
			ulong value = unchecked((ulong)_this);
			return ToHexString(value);
		}

		public static string ToHexString(this ulong _this)
		{
			ulong reverse = (_this & 0x00000000000000FFUL) << 56 | (_this & 0x000000000000FF00UL) << 40 |
					(_this & 0x0000000000FF0000UL) << 24 | (_this & 0x00000000FF000000UL) << 8 |
					(_this & 0x000000FF00000000UL) >> 8 | (_this & 0x0000FF0000000000UL) >> 24 |
					(_this & 0x00FF000000000000UL) >> 40 | (_this & 0xFF00000000000000UL) >> 56;
			return reverse.ToString("x16");
		}

		public static string ToHexString(this float _this)
		{
			uint value = BitConverter.SingleToUInt32Bits(_this);
			return ToHexString(value);
		}

		public static string ToHexString(this double _this)
		{
			ulong value = BitConverter.DoubleToUInt64Bits(_this);
			return ToHexString(value);
		}

		public static int ToClosestInt(this long _this)
		{
			if (_this > int.MaxValue)
			{
				return int.MaxValue;
			}
			if (_this < int.MinValue)
			{
				return int.MinValue;
			}
			return unchecked((int)_this);
		}
	}
}
