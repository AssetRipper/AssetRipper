using System.Text;

namespace AssetRipper.Yaml.Extensions
{
	public static class StringBuilderExtensions
	{
		static StringBuilderExtensions()
		{
			for (int i = 0; i <= byte.MaxValue; i++)
			{
				ByteHexRepresentations[i] = i.ToString("x2");
			}
		}

		public static StringBuilder AppendHex(this StringBuilder _this, byte value)
		{
			_this.Append(ByteHexRepresentations[value]);
			return _this;
		}

		public static StringBuilder AppendHex(this StringBuilder _this, ushort value)
		{
			_this.Append(ByteHexRepresentations[(value >> 0) & 0xFF]);
			_this.Append(ByteHexRepresentations[(value >> 8) & 0xFF]);
			return _this;
		}

		public static StringBuilder AppendHex(this StringBuilder _this, short value)
		{
			return _this.AppendHex(unchecked((ushort)value));
		}

		public static StringBuilder AppendHex(this StringBuilder _this, uint value)
		{
			_this.Append(ByteHexRepresentations[unchecked((int)(value >> 0) & 0xFF)]);
			_this.Append(ByteHexRepresentations[unchecked((int)(value >> 8) & 0xFF)]);
			_this.Append(ByteHexRepresentations[unchecked((int)(value >> 16) & 0xFF)]);
			_this.Append(ByteHexRepresentations[unchecked((int)(value >> 24) & 0xFF)]);
			return _this;
		}

		public static StringBuilder AppendHex(this StringBuilder _this, int value)
		{
			return _this.AppendHex(unchecked((uint)value));
		}

		public static StringBuilder AppendHex(this StringBuilder _this, ulong value)
		{
			_this.Append(ByteHexRepresentations[unchecked((int)(value >> 0) & 0xFF)]);
			_this.Append(ByteHexRepresentations[unchecked((int)(value >> 8) & 0xFF)]);
			_this.Append(ByteHexRepresentations[unchecked((int)(value >> 16) & 0xFF)]);
			_this.Append(ByteHexRepresentations[unchecked((int)(value >> 24) & 0xFF)]);
			_this.Append(ByteHexRepresentations[unchecked((int)(value >> 32) & 0xFF)]);
			_this.Append(ByteHexRepresentations[unchecked((int)(value >> 40) & 0xFF)]);
			_this.Append(ByteHexRepresentations[unchecked((int)(value >> 48) & 0xFF)]);
			_this.Append(ByteHexRepresentations[unchecked((int)(value >> 56) & 0xFF)]);
			return _this;
		}

		public static StringBuilder AppendHex(this StringBuilder _this, long value)
		{
			return _this.AppendHex(unchecked((ulong)value));
		}

		public static StringBuilder AppendHex(this StringBuilder _this, float value)
		{
			return _this.AppendHex(BitConverter.SingleToUInt32Bits(value));
		}

		public static StringBuilder AppendHex(this StringBuilder _this, double value)
		{
			return _this.AppendHex(BitConverter.DoubleToUInt64Bits(value));
		}

		public static StringBuilder AppendIndent(this StringBuilder _this, int count)
		{
			for (int i = 0; i < count; i++)
			{
				_this.Append('\t');
			}

			return _this;
		}

		public static readonly string HexAlphabet = "0123456789abcdef";
		public static readonly string[] ByteHexRepresentations = new string[256];
	}
}
