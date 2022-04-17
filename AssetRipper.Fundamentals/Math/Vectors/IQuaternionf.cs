using AssetRipper.Core.IO.Asset;
using System;

namespace AssetRipper.Core.Math.Vectors
{
	public interface IQuaternionf : IAsset
	{
		float X { get; set; }
		float Y { get; set; }
		float Z { get; set; }
		float W { get; set; }
	}

	public static class QuaternionfExtensions
	{
		public static float GetAt(this IQuaternionf quaternion, int index)
		{
			return index switch
			{
				0 => quaternion.X,
				1 => quaternion.Y,
				2 => quaternion.Z,
				3 => quaternion.W,
				_ => throw new IndexOutOfRangeException($"Index {index} is out of bound"),
			};
		}

		public static void SetAt(this IQuaternionf quaternion, int index, float value)
		{
			switch (index)
			{
				case 0:
					quaternion.X = value;
					return;
				case 1:
					quaternion.Y = value;
					return;
				case 2:
					quaternion.Z = value;
					return;
				case 3:
					quaternion.W = value;
					return;
				default:
					throw new IndexOutOfRangeException($"Index {index} is out of bound");
			}
		}

		public static void FlipSignAt(this IQuaternionf quaternion, int index)
		{
			switch (index)
			{
				case 0:
					quaternion.X = -quaternion.X;
					return;
				case 1:
					quaternion.Y = -quaternion.Y;
					return;
				case 2:
					quaternion.Z = -quaternion.Z;
					return;
				case 3:
					quaternion.W = -quaternion.W;
					return;
				default:
					throw new IndexOutOfRangeException($"Index {index} is out of bound");
			}
		}

		public static Vector3f ToEulerAngle(this IQuaternionf quaternion, bool asDegrees = true)
		{
			Vector3f euler = new Vector3f();
			quaternion.ToEulerAngle(euler, asDegrees);
			return euler;
		}

		/// <summary>
		/// Converts 
		/// </summary>
		/// <param name="quaternion">The source quaternion</param>
		/// <param name="destination">The destination vector for the Euler values to</param>
		/// <param name="asDegrees">Convert the result to degrees? If false, radians are used.</param>
		public static void ToEulerAngle(this IQuaternionf quaternion, IVector3f destination, bool asDegrees)
		{
			double eax;
			double eay;
			double eaz;

			float qx = quaternion.X;
			float qy = -quaternion.Y;
			float qz = -quaternion.Z;
			float qw = quaternion.W;

			double[,] M = new double[4, 4];

			double Nq = qx * qx + qy * qy + qz * qz + qw * qw;
			double s = Nq > 0.0 ? 2.0 / Nq : 0.0;
			double xs = qx * s, ys = qy * s, zs = qz * s;
			double wx = qw * xs, wy = qw * ys, wz = qw * zs;
			double xx = qx * xs, xy = qx * ys, xz = qx * zs;
			double yy = qy * ys, yz = qy * zs, zz = qz * zs;

			M[0, 0] = 1.0 - (yy + zz); M[0, 1] = xy - wz; M[0, 2] = xz + wy;
			M[1, 0] = xy + wz; M[1, 1] = 1.0 - (xx + zz); M[1, 2] = yz - wx;
			M[2, 0] = xz - wy; M[2, 1] = yz + wx; M[2, 2] = 1.0 - (xx + yy);
			M[3, 0] = M[3, 1] = M[3, 2] = M[0, 3] = M[1, 3] = M[2, 3] = 0.0; M[3, 3] = 1.0;

			double test = System.Math.Sqrt(M[0, 0] * M[0, 0] + M[1, 0] * M[1, 0]);
			if (test > 16 * 1.19209290E-07F)//FLT_EPSILON
			{
				eax = System.Math.Atan2(M[2, 1], M[2, 2]);
				eay = System.Math.Atan2(-M[2, 0], test);
				eaz = System.Math.Atan2(M[1, 0], M[0, 0]);
			}
			else
			{
				eax = System.Math.Atan2(-M[1, 2], M[1, 1]);
				eay = System.Math.Atan2(-M[2, 0], test);
				eaz = 0;
			}

			destination.X = GetAngle(eax, asDegrees);
			destination.Y = GetAngle(eay, asDegrees);
			destination.Z = GetAngle(eaz, asDegrees);
		}

		private static float GetAngle(double radians, bool convertToDegrees)
		{
			return (float)(convertToDegrees ? (radians * 180.0 / System.Math.PI) : radians);
		}

		public static double Dot(this IQuaternionf a, IQuaternionf b)
		{
			return (double)a.X * b.X + (double)a.Y * b.Y + (double)a.Z * b.Z + (double)a.W * b.W;
		}

		public static bool IsUnitQuaternion(this IQuaternionf a)
		{
			return (a.X * a.X + a.Y * a.Y + a.Z * a.Z + a.W * a.W) > 1d - kEpsilon;
		}

		public static bool IsEqualUsingDot(this IQuaternionf a, IQuaternionf b)
		{
			return a.Dot(b) > 1.0f - kEpsilon;
		}

		private const float kEpsilon = 0.00001F;
	}
}
