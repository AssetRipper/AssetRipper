namespace AssetRipper.Numerics;

/// <summary>
/// Represents an RGBA color value as four single precision floating point values.
/// </summary>
public readonly record struct ColorFloat(Vector4 Vector)
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ColorFloat"/> struct with RGBA values.
	/// </summary>
	/// <param name="R">The red value, between 0 and 1.</param>
	/// <param name="G">The green value, between 0 and 1.</param>
	/// <param name="B">The blue value, between 0 and 1.</param>
	/// <param name="A">The alpha value, between 0 and 1.</param>
	public ColorFloat(float R, float G, float B, float A) : this(new Vector4(R, G, B, A)) { }

	/// <summary>
	/// Gets the red value, between 0 and 1.
	/// </summary>
	public float R => Vector.X;

	/// <summary>
	/// Gets the green value, between 0 and 1.
	/// </summary>
	public float G => Vector.Y;

	/// <summary>
	/// Gets the blue value, between 0 and 1.
	/// </summary>
	public float B => Vector.Z;

	/// <summary>
	/// Gets the alpha value, between 0 and 1.
	/// </summary>
	public float A => Vector.W;

	/// <summary>
	/// Clamps the color values of the color within the range of 0 to 1 and returns the resulting color.
	/// </summary>
	/// <returns>A new instance of <see cref="ColorFloat"/> with clamped color values.</returns>
	public ColorFloat Clamp()
	{
		return new ColorFloat(
			Math.Min(Math.Max(R, 0), 1),
			Math.Min(Math.Max(G, 0), 1),
			Math.Min(Math.Max(B, 0), 1),
			Math.Min(Math.Max(A, 0), 1)
		);
	}

	/// <summary>
	/// Adds two <see cref="ColorFloat"/> values and returns the resulting color.
	/// </summary>
	/// <param name="a">The first color to add.</param>
	/// <param name="b">The second color to add.</param>
	/// <returns>A new instance of <see cref="ColorFloat"/> that is the result of adding the two input colors.</returns>
	public static ColorFloat operator +(ColorFloat a, ColorFloat b)
	{
		return new ColorFloat(a.Vector + b.Vector);
	}

	/// <summary>
	/// Subtracts two <see cref="ColorFloat"/> values and returns the resulting color.
	/// </summary>
	/// <param name="a">The color to subtract from.</param>
	/// <param name="b">The color to subtract.</param>
	/// <returns>A new instance of <see cref="ColorFloat"/> that is the result of subtracting the second color from the first color.</returns>
	public static ColorFloat operator -(ColorFloat a, ColorFloat b)
	{
		return new ColorFloat(a.Vector - b.Vector);
	}

	/// <summary>
	/// Multiplies a <see cref="ColorFloat"/> values by a scalar and returns the resulting color.
	/// </summary>
	/// <param name="a">The color to multiply.</param>
	/// <param name="b">The scalar value to multiply the color by.</param>
	/// <returns>A new instance of <see cref="ColorFloat"/> that is the result of multiplying the color with the scalar value.</returns>
	public static ColorFloat operator *(ColorFloat a, float b)
	{
		return new ColorFloat(a.Vector * b);
	}

	/// <summary>
	/// Multiplies a <see cref="ColorFloat"/> values by a scalar and returns the resulting color.
	/// </summary>
	/// <param name="b">The scalar value to multiply the color by.</param>
	/// <param name="a">The color to multiply.</param>
	/// <returns>A new instance of <see cref="ColorFloat"/> that is the result of multiplying the color with the scalar value.</returns>
	public static ColorFloat operator *(float b, ColorFloat a)
	{
		return new ColorFloat(a.Vector * b);
	}

	/// <summary>
	/// Divides a <see cref="ColorFloat"/> values by a scalar and returns the resulting color.
	/// </summary>
	/// <param name="a">The color to divide.</param>
	/// <param name="b">The scalar value to divide the color by.</param>
	/// <returns>A new instance of <see cref="ColorFloat"/> that is the result of dividing the color with the scalar value.</returns>
	public static ColorFloat operator /(ColorFloat a, float b)
	{
		return new ColorFloat(a.Vector / b);
	}

	/// <summary>
	/// Gets a pre-defined color with RGBA values of (0, 0, 0, 1).
	/// </summary>
	public static ColorFloat Black => new ColorFloat(0, 0, 0, 1);

	/// <summary>
	/// Gets a pre-defined color with RGBA values of (1, 1, 1, 1).
	/// </summary>
	public static ColorFloat White => new ColorFloat(1, 1, 1, 1);

	/// <inheritdoc/>
	public override string ToString()
	{
		return $"[R:{R:0.00} G:{G:0.00} B:{B:0.00} A:{A:0.00}]";
	}
}
