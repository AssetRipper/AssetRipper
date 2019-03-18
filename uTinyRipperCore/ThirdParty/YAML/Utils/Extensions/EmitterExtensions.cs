namespace uTinyRipper.YAML
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
			return WriteHex(_this, unchecked((ushort)value));
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
			return WriteHex(_this, unchecked((uint)value));
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
			return WriteHex(_this, unchecked((ulong)value));
		}

		public static Emitter WriteHex(this Emitter _this, float value)
		{
			return WriteHex(_this, BitConverterExtensions.ToUInt32(value));
		}

		public static Emitter WriteHex(this Emitter _this, double value)
		{
			return WriteHex(_this, BitConverterExtensions.ToUInt64(value));
		}

		private static readonly string HexAlphabet = "0123456789ABCDEF";
	}
}
