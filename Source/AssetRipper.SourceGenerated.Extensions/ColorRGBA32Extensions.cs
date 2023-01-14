using AssetRipper.SourceGenerated.Subclasses.ColorRGBA32;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class ColorRGBA32Extensions
	{
		public static void SetValues(this IColorRGBA32 color, byte r, byte g, byte b, byte a)
		{
			color.Rgba = unchecked((uint)(r | g << 8 | b << 16 | a << 24));
		}

		public static void SetAsBlack(this IColorRGBA32 color) => color.SetValues(byte.MinValue, byte.MinValue, byte.MinValue, byte.MaxValue);

		public static void SetAsWhite(this IColorRGBA32 color) => color.SetValues(uint.MaxValue);

		public static byte GetR(this IColorRGBA32 color)
		{
			return unchecked((byte)color.Rgba);
		}

		public static byte GetG(this IColorRGBA32 color)
		{
			return unchecked((byte)(color.Rgba >> 8));
		}

		public static byte GetB(this IColorRGBA32 color)
		{
			return unchecked((byte)(color.Rgba >> 16));
		}

		public static byte GetA(this IColorRGBA32 color)
		{
			return unchecked((byte)(color.Rgba >> 24));
		}
	}
}
