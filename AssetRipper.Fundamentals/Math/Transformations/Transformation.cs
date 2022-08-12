using System.Numerics;

namespace AssetRipper.Core.Math.Transformations
{
	public readonly record struct Transformation(Matrix4x4 Matrix)
	{
		public Transformation(Vector3 translation, Quaternion rotation, Vector3 scale) : this(ConvertToMatrix(translation, rotation, scale)) { }
		public static Transformation Identity { get; } = new Transformation(Matrix4x4.Identity);
		/// <summary>
		/// Used for switching coordinates from left-handedness to right-handedness
		/// </summary>
		public static Transformation IdentityWithInvertedX { get; } = new Transformation(new Matrix4x4(-1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1));

		public static Vector3 operator *(Vector3 position, Transformation transform)
		{
			return Vector3.Transform(position, transform.Matrix);
		}
		
		public static Transformation operator *(Transformation left, Transformation right)
		{
			return new Transformation(left.Matrix * right.Matrix);
		}

		public static Transformation Create(Vector3 translation, Quaternion rotation, Vector3 scale)
		{
			return new Transformation(translation, rotation, scale);
		}

		private static Matrix4x4 ConvertToMatrix(Vector3 translation, Quaternion rotation, Vector3 scale)
		{
			return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation);
		}

		public static Transformation CreateInverse(Vector3 translation, Quaternion rotation, Vector3 scale)
		{
			Matrix4x4 matrix = Matrix4x4.CreateTranslation(Vector3.Negate(translation)) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(rotation)) * Matrix4x4.CreateScale(Vector3.One / scale);
			return new Transformation(matrix);
		}

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

		public Transformation Transpose()
		{
			return new Transformation(Matrix4x4.Transpose(Matrix));
		}
		
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
	}
}
