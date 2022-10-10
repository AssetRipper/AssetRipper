using System.Numerics;
using System.Runtime.CompilerServices;

namespace AssetRipper.Core.Math.Vectors
{
	public static class VectorExtensions
	{
		public static Vector3 AsVector3(this Vector4 vector4)
		{
			return Unsafe.As<Vector4, Vector3>(ref vector4);
		}

		public static Vector3 InvertX(this Vector3 vector)
		{
			return new Vector3(-vector.X, vector.Y, vector.Z);
		}
	}
}
