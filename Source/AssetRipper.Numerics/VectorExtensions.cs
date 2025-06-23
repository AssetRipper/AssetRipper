using System.Runtime.CompilerServices;

namespace AssetRipper.Numerics;

public static class VectorExtensions
{
	public static Vector2 AsVector2(this Vector3 vector3)
	{
		return Unsafe.As<Vector3, Vector2>(ref vector3);
	}

	public static Vector3 AsVector3(this Vector2 vector2)
	{
		return new Vector3(vector2, 0);
	}

	public static Vector3 InvertX(this Vector3 vector)
	{
		return new Vector3(-vector.X, vector.Y, vector.Z);
	}
}
