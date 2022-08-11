namespace AssetRipper.Core.Math.Colors
{
	public readonly record struct Color32(byte R, byte G, byte B, byte A)
	{
		private const float ByteMaxValue = byte.MaxValue;
		
		public uint Rgba => unchecked((uint)(R | (G << 8) | (B << 16) | (A << 24)));

		public static explicit operator ColorFloat(Color32 color)
		{
			return new ColorFloat(color.R / ByteMaxValue, color.G / ByteMaxValue, color.B / ByteMaxValue, color.A / ByteMaxValue);
		}
		
		public static explicit operator Color32(ColorFloat color)
		{
			byte r = ConvertFloatToByte(color.R);
			byte g = ConvertFloatToByte(color.G);
			byte b = ConvertFloatToByte(color.B);
			byte a = ConvertFloatToByte(color.A);
			return new Color32(r, g, b, a);
		}

		private static byte ConvertFloatToByte(float value)
		{
			if (float.IsNaN(value))
			{
				return byte.MinValue;
			}

			float scaledValue = value * byte.MaxValue;
			if (scaledValue <= 0)
			{
				return byte.MinValue;
			}
			else if (scaledValue >= byte.MaxValue)
			{
				return byte.MaxValue;
			}
			else
			{
				return (byte)scaledValue;
			}
		}
		
		public static Color32 Black => new Color32(byte.MinValue, byte.MinValue, byte.MinValue, byte.MaxValue);
		public static Color32 White => new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		public override string ToString()
		{
			return $"[R:{R} G:{G} B:{B} A:{A}]";
		}
	}
}
