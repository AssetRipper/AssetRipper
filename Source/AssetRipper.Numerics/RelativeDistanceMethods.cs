using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AssetRipper.Numerics;

/// <summary>
/// A collection of methods calculating the relative distance between two points.
/// All methods return a <see langword="float"/> between 0 and 1 (inclusive).
/// 0 indicates equality and 1 indicates great distance
/// For the array methods, the return value is an average of the relative distances for the individual points.
/// </summary>
public static class RelativeDistanceMethods
{
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static float RelativeDistance(float x1, float x2)
	{
		return x1 == x2 ? 0 : MathF.Abs(x1 - x2) / (MathF.Abs(x1) + MathF.Abs(x2));
	}

	//between zero and positive infinity
	public static float RelativeDistance2(float x1, float x2)
	{
		float ratio = (x1 - x2) / (x1 + x2);
		return ratio * ratio;
	}

	public static float RelativeDistance(Vector2 v1, Vector2 v2)
	{
		return v1 == v2 ? 0 : Vector2.Distance(v1, v2) / (v1.Length() + v2.Length());
	}

	public static float RelativeDistance2(Vector2 v1, Vector2 v2)
	{
		return Vector2.DistanceSquared(v1, v2) / (v1 + v2).LengthSquared();
	}

	public static float RelativeDistance(Vector3 v1, Vector3 v2)
	{
		return v1 == v2 ? 0 : Vector3.Distance(v1, v2) / (v1.Length() + v2.Length());
	}

	public static float RelativeDistance(Vector4 v1, Vector4 v2)
	{
		return v1 == v2 ? 0 : Vector4.Distance(v1, v2) / (v1.Length() + v2.Length());
	}

	public static void RelativeDistance(float[] x1, float[] x2, out float sum, out int count)
	{
		Debug.Assert(x1.Length == x2.Length);
		sum = 0;
		for (int i = 0; i < x1.Length; i++)
		{
			sum += RelativeDistance(x1[i], x2[i]);
		}
		count = x1.Length;
	}

	public static void RelativeDistance(Vector2[] v1, Vector2[] v2, out float sum, out int count)
	{
		Debug.Assert(v1.Length == v2.Length);
		sum = 0;
		for (int i = 0; i < v1.Length; i++)
		{
			sum += RelativeDistance(v1[i], v2[i]);
		}
		count = v1.Length;
	}

	public static void RelativeDistance(Vector3[] v1, Vector3[] v2, out float sum, out int count)
	{
		Debug.Assert(v1.Length == v2.Length);
		sum = 0;
		for (int i = 0; i < v1.Length; i++)
		{
			sum += RelativeDistance(v1[i], v2[i]);
		}
		count = v1.Length;
	}

	public static void RelativeDistance(Vector4[] v1, Vector4[] v2, out float sum, out int count)
	{
		Debug.Assert(v1.Length == v2.Length);
		sum = 0;
		for (int i = 0; i < v1.Length; i++)
		{
			sum += RelativeDistance(v1[i], v2[i]);
		}
		count = v1.Length;
	}

	public static void RelativeDistance(ColorFloat[] v1, ColorFloat[] v2, out float sum, out int count)
	{
		Debug.Assert(v1.Length == v2.Length);
		sum = 0;
		for (int i = 0; i < v1.Length; i++)
		{
			sum += RelativeDistance(v1[i], v2[i]);
		}
		count = v1.Length;
	}

	public static float RelativeDistance(ColorFloat v1, ColorFloat v2)
	{
		return RelativeDistance(v1.Vector, v2.Vector);
	}

	public static void RelativeDistance(BoneWeight4[] v1, BoneWeight4[] v2, out float sum, out int count)
	{
		Debug.Assert(v1.Length == v2.Length);
		sum = 0;
		for (int i = 0; i < v1.Length; i++)
		{
			sum += RelativeDistance(v1[i], v2[i]);
		}
		count = v1.Length;
	}

	public static float RelativeDistance(BoneWeight4 v1, BoneWeight4 v2)
	{
		return (RelativeDistance(v1.Weight0, v2.Weight0)
			+ RelativeDistance(v1.Weight1, v2.Weight1)
			+ RelativeDistance(v1.Weight2, v2.Weight2)
			+ RelativeDistance(v1.Weight3, v2.Weight3)) / 4;
	}

	public static void RelativeDistance(Matrix4x4[] v1, Matrix4x4[] v2, out float sum, out int count)
	{
		Debug.Assert(v1.Length == v2.Length);
		sum = 0;
		for (int i = 0; i < v1.Length; i++)
		{
			sum += RelativeDistance(v1[i], v2[i]);
		}
		count = v1.Length;
	}

	public static float RelativeDistance(Matrix4x4 v1, Matrix4x4 v2)
	{
		return (RelativeDistance(v1.M11, v2.M11)
			+ RelativeDistance(v1.M12, v2.M12)
			+ RelativeDistance(v1.M13, v2.M13)
			+ RelativeDistance(v1.M14, v2.M14)
			+ RelativeDistance(v1.M21, v2.M21)
			+ RelativeDistance(v1.M22, v2.M22)
			+ RelativeDistance(v1.M23, v2.M23)
			+ RelativeDistance(v1.M24, v2.M24)
			+ RelativeDistance(v1.M31, v2.M31)
			+ RelativeDistance(v1.M32, v2.M32)
			+ RelativeDistance(v1.M33, v2.M33)
			+ RelativeDistance(v1.M34, v2.M34)
			+ RelativeDistance(v1.M41, v2.M41)
			+ RelativeDistance(v1.M42, v2.M42)
			+ RelativeDistance(v1.M43, v2.M43)
			+ RelativeDistance(v1.M44, v2.M44)) / 16;
	}
}
