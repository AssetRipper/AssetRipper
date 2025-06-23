namespace AssetRipper.Numerics;

/// <summary>
/// Represents a transformation composed of a translation, rotation, and scale
/// </summary>
public readonly record struct Transformation(Matrix4x4 Matrix)
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Transformation"/> class with the provided translation, rotation, and scale
	/// </summary>
	/// <param name="translation">The translation component of the transformation</param>
	/// <param name="rotation">The rotation component of the transformation</param>
	/// <param name="scale">The scale component of the transformation</param>
	public Transformation(Vector3 translation, Quaternion rotation, Vector3 scale) : this(ConvertToMatrix(translation, rotation, scale)) { }

	/// <summary>
	/// Gets an <see cref="Identity"/> transformation that does nothing
	/// </summary>
	public static Transformation Identity { get; } = new Transformation(Matrix4x4.Identity);

	/// <summary>
	/// Gets an <see cref="Identity"/> transformation that switches coordinates from left-handedness to right-handedness
	/// </summary>
	/// <remarks>
	/// This is useful when converting coordinates from Unity to Gltf
	/// </remarks>
	public static Transformation IdentityWithInvertedX { get; } = new Transformation(new Matrix4x4(-1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1));

	/// <summary>
	/// Transforms a position vector by the transformation matrix
	/// </summary>
	/// <param name="position">The position vector to transform</param>
	/// <param name="transform">The transformation to apply</param>
	/// <returns>The transformed vector</returns>
	public static Vector3 operator *(Vector3 position, Transformation transform)
	{
		return Vector3.Transform(position, transform.Matrix);
	}

	/// <summary>
	/// Composes two transformations
	/// </summary>
	/// <param name="left">The first transformation to compose</param>
	/// <param name="right">The second transformation to compose</param>
	/// <returns>The composed transformation</returns>
	public static Transformation operator *(Transformation left, Transformation right)
	{
		return new Transformation(left.Matrix * right.Matrix);
	}

	/// <summary>
	/// Implicitly converts a <see cref="Transformation"/> to a <see cref="Matrix4x4"/>
	/// </summary>
	/// <param name="transformation">The transformation to convert</param>
	public static implicit operator Matrix4x4(Transformation transformation)
	{
		return transformation.Matrix;
	}

	/// <summary>
	/// Implicitly converts a <see cref="Matrix4x4"/> to a <see cref="Transformation"/>
	/// </summary>
	/// <param name="matrix">The matrix to convert</param>
	public static implicit operator Transformation(Matrix4x4 matrix)
	{
		return new Transformation(matrix);
	}

	/// <summary>
	/// Creates a new <see cref="Transformation"/> with the provided translation, rotation, and scale
	/// </summary>
	/// <param name="translation">The translation component of the transformation</param>
	/// <param name="rotation">The rotation component of the transformation</param>
	/// <param name="scale">The scale component of the transformation</param>
	public static Transformation Create(Vector3 translation, Quaternion rotation, Vector3 scale)
	{
		return new Transformation(translation, rotation, scale);
	}

	private static Matrix4x4 ConvertToMatrix(Vector3 translation, Quaternion rotation, Vector3 scale)
	{
		return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation);
	}

	/// <summary>
	/// Creates a new <see cref="Transformation"/> with the inverse of the provided translation, rotation, and scale
	/// </summary>
	/// <param name="translation">The translation component of the transformation</param>
	/// <param name="rotation">The rotation component of the transformation</param>
	/// <param name="scale">The scale component of the transformation</param>
	public static Transformation CreateInverse(Vector3 translation, Quaternion rotation, Vector3 scale)
	{
		Vector3 inverseTranslation = Vector3.Negate(translation);
		Quaternion inverseRotation = InvertQuaternion(rotation);
		Vector3 inverseScale = Vector3.One / scale;
		Matrix4x4 matrix = Matrix4x4.CreateTranslation(inverseTranslation) * Matrix4x4.CreateFromQuaternion(inverseRotation) * Matrix4x4.CreateScale(inverseScale);
		return new Transformation(matrix);
	}

	private static Quaternion InvertQuaternion(Quaternion rotation)
	{
		if (rotation.IsZero())
		{
			return Quaternion.Identity;
		}
		else
		{
			return Quaternion.Inverse(rotation);
		}
	}

	/// <summary>
	/// Inverts the transformation
	/// </summary>
	/// <returns>The inverted transformation</returns>
	public Transformation Invert()
	{
		if (Matrix4x4.Invert(Matrix, out Matrix4x4 inverted))
		{
			return new Transformation(inverted);
		}
		else
		{
			throw new Exception("Could not invert matrix");
		}
	}

	/// <summary>
	/// Transposes the transformation
	/// </summary>
	/// <returns>The transposed transformation</returns>
	public Transformation Transpose()
	{
		return new Transformation(Matrix4x4.Transpose(Matrix));
	}

	/// <summary>
	/// Removes the translation component of the transformation
	/// </summary>
	/// <returns>The transformed vector</returns>
	public Transformation RemoveTranslation()
	{
		return new Transformation(ResetFourthRow(Matrix));
	}

	private static Matrix4x4 ResetFourthRow(Matrix4x4 matrix)
	{
		return new Matrix4x4(
			matrix.M11, matrix.M12, matrix.M13, matrix.M14,
			matrix.M21, matrix.M22, matrix.M23, matrix.M24,
			matrix.M31, matrix.M32, matrix.M33, matrix.M34,
			0, 0, 0, 1);
	}

	public override string ToString() => Matrix.ToString();
}
