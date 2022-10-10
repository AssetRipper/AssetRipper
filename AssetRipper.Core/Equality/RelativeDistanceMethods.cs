using System.Diagnostics;
using System.Numerics;

namespace AssetRipper.Core.Equality
{
	/// <summary>
	/// A collection of methods calculating the relative distance between two points.
	/// All methods return a <see langword="float"/> between 0 and 1 (inclusive).
	/// 0 indicates equality and 1 indicates great distance
	/// For the array methods, the return value is an average of the relative distances for the individual points.
	/// </summary>
	internal static class RelativeDistanceMethods
	{
		public static float RelativeDistance(float x1, float x2)
		{
			return MathF.Abs(x1 - x2) / (MathF.Abs(x1) + MathF.Abs(x2));
		}
		//between zero and positive infinity
		public static float RelativeDistance2(float x1, float x2)
		{
			float ratio = (x1 - x2) / (x1 + x2);
			return ratio * ratio;
		}

		public static float RelativeDistance(Vector2 v1, Vector2 v2)
		{
			return Vector2.Distance(v1, v2) / (v1.Length() + v2.Length());
		}

		public static float RelativeDistance2(Vector2 v1, Vector2 v2)
		{
			return Vector2.DistanceSquared(v1, v2) / (v1 + v2).LengthSquared();
		}

		public static float RelativeDistance(Vector3 v1, Vector3 v2)
		{
			return Vector3.Distance(v1, v2) / (v1.Length() + v2.Length());
		}

		public static float RelativeDistance(Vector4 v1, Vector4 v2)
		{
			return Vector4.Distance(v1, v2) / (v1.Length() + v2.Length());
		}

		public static float RelativeDistance(float[] x1, float[] x2)
		{
			Debug.Assert(x1.Length == x2.Length);
			Debug.Assert(x1.Length != 0);
			float sum = 0;
			for (int i = 0; i < x1.Length; i++)
			{
				sum += RelativeDistance(x1[i], x2[i]);
			}
			return sum / x1.Length;
		}

		public static float RelativeDistance(Vector2[] v1, Vector2[] v2)
		{
			Debug.Assert(v1.Length == v2.Length);
			Debug.Assert(v1.Length != 0);
			float sum = 0;
			for (int i = 0; i < v1.Length; i++)
			{
				sum += RelativeDistance(v1[i], v2[i]);
			}
			return sum / v1.Length;
		}

		public static float RelativeDistance(Vector3[] v1, Vector3[] v2)
		{
			Debug.Assert(v1.Length == v2.Length);
			Debug.Assert(v1.Length != 0);
			float sum = 0;
			for (int i = 0; i < v1.Length; i++)
			{
				sum += RelativeDistance(v1[i], v2[i]);
			}
			return sum / v1.Length;
		}

		public static float RelativeDistance(Vector4[] v1, Vector4[] v2)
		{
			Debug.Assert(v1.Length == v2.Length);
			Debug.Assert(v1.Length != 0);
			float sum = 0;
			for (int i = 0; i < v1.Length; i++)
			{
				sum += RelativeDistance(v1[i], v2[i]);
			}
			return sum / v1.Length;
		}
	}
}
