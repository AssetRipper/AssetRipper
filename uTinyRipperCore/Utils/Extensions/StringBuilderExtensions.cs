using System.Text;

namespace uTinyRipper
{
	public static class StringBuilderExtensions
	{
		public static StringBuilder AppendHex(this StringBuilder _this, byte value)
		{
			_this.Append(HexAlphabet[value >> 4]);
			_this.Append(HexAlphabet[value & 0xF]);
			return _this;
		}

		public static StringBuilder AppendHex(this StringBuilder _this, ushort value)
		{
			_this.Append(HexAlphabet[(value >> 4) & 0xF]);
			_this.Append(HexAlphabet[(value >> 0) & 0xF]);
			_this.Append(HexAlphabet[(value >> 12) & 0xF]);
			_this.Append(HexAlphabet[(value >> 8) & 0xF]);
			return _this;
		}

		public static StringBuilder AppendHex(this StringBuilder _this, short value)
		{
			return AppendHex(_this, unchecked((ushort)value));
		}

		public static StringBuilder AppendHex(this StringBuilder _this, uint value)
		{
			_this.Append(HexAlphabet[unchecked((int)(value >> 4) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 0) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 12) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 8) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 20) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 16) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 28) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 24) & 0xF)]);
			return _this;
		}

		public static StringBuilder AppendHex(this StringBuilder _this, int value)
		{
			return AppendHex(_this, unchecked((uint)value));
		}

		public static StringBuilder AppendHex(this StringBuilder _this, ulong value)
		{
			_this.Append(HexAlphabet[unchecked((int)(value >> 4) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 0) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 12) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 8) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 20) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 16) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 28) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 24) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 36) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 32) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 44) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 40) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 52) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 48) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 60) & 0xF)]);
			_this.Append(HexAlphabet[unchecked((int)(value >> 56) & 0xF)]);
			return _this;
		}

		public static StringBuilder AppendHex(this StringBuilder _this, long value)
		{
			return AppendHex(_this, unchecked((ulong)value));
		}

		public static StringBuilder AppendHex(this StringBuilder _this, float value)
		{
			return AppendHex(_this, BitConverterExtensions.ToUInt32(value));
		}

		public static StringBuilder AppendHex(this StringBuilder _this, double value)
		{
			return AppendHex(_this, BitConverterExtensions.ToUInt64(value));
		}

		public static StringBuilder AppendIntent(this StringBuilder _this, int count)
		{
			for(int i = 0; i < count; i++)
			{
				_this.Append('\t');
			}
			return _this;
		}

		private static readonly string HexAlphabet = "0123456789abcdef";
	}
}
