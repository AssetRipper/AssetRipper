namespace AssetRipper.Yaml.Extensions
{
	internal static class EmitterExtensions
	{
		public static Emitter WriteHex(this Emitter _this, byte value)
		{
			_this.Write(HexAlphabet[value >> 4]);
			_this.Write(HexAlphabet[value & 0xF]);
			return _this;
		}

		public static Emitter WriteHex(this Emitter _this, ushort value)
		{
			_this.Write(HexAlphabet[(value >> 4) & 0xF]);
			_this.Write(HexAlphabet[(value >> 0) & 0xF]);
			_this.Write(HexAlphabet[(value >> 12) & 0xF]);
			_this.Write(HexAlphabet[(value >> 8) & 0xF]);
			return _this;
		}

		public static Emitter WriteHex(this Emitter _this, short value)
		{
			return _this.WriteHex(unchecked((ushort)value));
		}

		public static Emitter WriteHex(this Emitter _this, uint value)
		{
			_this.Write(HexAlphabet[unchecked((int)(value >> 4) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 0) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 12) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 8) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 20) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 16) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 28) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 24) & 0xF)]);
			return _this;
		}

		public static Emitter WriteHex(this Emitter _this, int value)
		{
			return _this.WriteHex(unchecked((uint)value));
		}

		public static Emitter WriteHex(this Emitter _this, ulong value)
		{
			_this.Write(HexAlphabet[unchecked((int)(value >> 4) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 0) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 12) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 8) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 20) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 16) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 28) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 24) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 36) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 32) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 44) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 40) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 52) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 48) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 60) & 0xF)]);
			_this.Write(HexAlphabet[unchecked((int)(value >> 56) & 0xF)]);
			return _this;
		}

		public static Emitter WriteHex(this Emitter _this, long value)
		{
			return _this.WriteHex(unchecked((ulong)value));
		}

		public static Emitter WriteHex(this Emitter _this, float value)
		{
			return _this.WriteHex(BitConverter.SingleToUInt32Bits(value));
		}

		public static Emitter WriteHex(this Emitter _this, double value)
		{
			return _this.WriteHex(BitConverter.DoubleToUInt64Bits(value));
		}

		private static readonly string HexAlphabet = "0123456789ABCDEF";
	}
}
