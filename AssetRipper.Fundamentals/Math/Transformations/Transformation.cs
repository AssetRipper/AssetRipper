using System.Numerics;

namespace AssetRipper.Core.Math.Transformations
{
	public readonly record struct Transformation(Matrix4x4 Matrix)
	{
		public Transformation(Vector3 position, Quaternion rotation, Vector3 scale) : this(ConvertToMatrix(position, rotation, scale)) { }
		public static Transformation Identity { get; } = new Transformation(Matrix4x4.Identity);
		
		public static Vector3 operator *(Vector3 position, Transformation transform)
		{
			return Vector3.Transform(position, transform.Matrix);
		}
		
		public static Transformation operator *(Transformation left, Transformation right)
		{
			return new Transformation(left.Matrix * right.Matrix);
		}
		
		private static Matrix4x4 ConvertToMatrix(Vector3 position, Quaternion rotation, Vector3 scale)
		{
			return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);
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
	}
}
