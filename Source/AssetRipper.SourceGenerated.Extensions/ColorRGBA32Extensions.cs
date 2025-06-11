using AssetRipper.Numerics;
using AssetRipper.SourceGenerated.Subclasses.ColorRGBA32;

namespace AssetRipper.SourceGenerated.Extensions;

public static class ColorRGBA32Extensions
{
	extension(ColorRGBA32 color)
	{
		public void SetValues(byte r, byte g, byte b, byte a)
		{
			color.Rgba = unchecked((uint)(r | g << 8 | b << 16 | a << 24));
		}

		public void SetAsBlack() => color.SetValues(byte.MinValue, byte.MinValue, byte.MinValue, byte.MaxValue);

		public void SetAsWhite() => color.SetValues(uint.MaxValue);

		public byte R
		{
			get => unchecked((byte)color.Rgba);
			set => color.SetValues(value, color.G, color.B, color.A);
		}

		public byte G
		{
			get => unchecked((byte)(color.Rgba >> 8));
			set => color.SetValues(color.R, value, color.B, color.A);
		}

		public byte B
		{
			get => unchecked((byte)(color.Rgba >> 16));
			set => color.SetValues(color.R, color.G, value, color.A);
		}

		public byte A
		{
			get => unchecked((byte)(color.Rgba >> 24));
			set => color.SetValues(color.R, color.G, color.B, value);
		}

		public ColorFloat ToColorFloat()
		{
			return (ColorFloat)Color32.FromRgba(color.Rgba);
		}
	}
}
