using System.Drawing;
using System.Runtime.CompilerServices;

namespace AssetRipper.Numerics;

/// <summary>
/// A struct that represents a color with 32 bits.
/// </summary>
public readonly record struct Color32(byte R, byte G, byte B, byte A)
{
	private const float ByteMaxValue = byte.MaxValue;

	/// <summary>
	/// Gets the color represented as a 32 bit unsigned integer.
	/// </summary>
	public uint Rgba
	{
		get
		{
			Color32 color = this;
			return Unsafe.As<Color32, uint>(ref color);
		}
	}

	/// <summary>
	/// Converts a 32 bit unsigned integer into a color.
	/// </summary>
	/// <param name="rgba">The value to convert</param>
	/// <returns>The resulting color.</returns>
	public static Color32 FromRgba(uint rgba)
	{
		return Unsafe.As<uint, Color32>(ref rgba);
	}

	/// <summary>
	/// Converts the color to a floating-point representation.
	/// </summary>
	/// <param name="color">The color to convert.</param>
	public static explicit operator ColorFloat(Color32 color)
	{
		return new ColorFloat(color.R / ByteMaxValue, color.G / ByteMaxValue, color.B / ByteMaxValue, color.A / ByteMaxValue);
	}

	/// <summary>
	/// Converts the floating-point representation to a color.
	/// </summary>
	/// <param name="color">The floating-point representation to convert.</param>
	public static explicit operator Color32(ColorFloat color)
	{
		byte r = ConvertFloatToByte(color.R);
		byte g = ConvertFloatToByte(color.G);
		byte b = ConvertFloatToByte(color.B);
		byte a = ConvertFloatToByte(color.A);
		return new Color32(r, g, b, a);
	}

	public static explicit operator Color(Color32 color)
	{
		int argb = (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
		return Color.FromArgb(argb);
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

	/// <summary>
	/// Gets the color black.
	/// </summary>
	public static Color32 Black => new Color32(byte.MinValue, byte.MinValue, byte.MinValue, byte.MaxValue);

	/// <summary>
	/// Gets the color white.
	/// </summary>
	public static Color32 White => new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	/// <summary>
	/// Gets the string representation of the color.
	/// </summary>
	/// <returns>The string representation of the color.</returns>
	public override string ToString()
	{
		return $"[R:{R} G:{G} B:{B} A:{A}]";
	}
}
