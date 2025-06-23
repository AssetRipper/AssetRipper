namespace AssetRipper.Numerics;

/// <summary>
/// Provides extension methods for the Quaternion struct.
/// </summary>
public static class QuaternionExtensions
{
	/// <summary>
	/// Returns the value of the specified index of the Quaternion.
	/// </summary>
	/// <param name="quaternion">The Quaternion instance.</param>
	/// <param name="index">The index of the Quaternion.</param>
	/// <returns>The value of the specified index of the Quaternion.</returns>
	public static float GetAt(this Quaternion quaternion, int index)
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

	/// <summary>
	/// Sets the value of the specified index of the Quaternion.
	/// </summary>
	/// <param name="quaternion">The Quaternion instance.</param>
	/// <param name="index">The index of the Quaternion.</param>
	/// <param name="value">The value to be set.</param>
	public static void SetAt(this ref Quaternion quaternion, int index, float value)
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

	/// <summary>
	/// Flips the sign of the value of the specified index of the Quaternion.
	/// </summary>
	/// <param name="quaternion">The Quaternion instance.</param>
	/// <param name="index">The index of the Quaternion.</param>
	public static void FlipSignAt(this ref Quaternion quaternion, int index)
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

	/// <summary>
	/// Converts a quaternion to Euler angles.
	/// </summary>
	/// <param name="quaternion">The Quaternion instance.</param>
	/// <param name="asDegrees">
	/// If true, the Euler angles will be in degrees. If false, radians are used.
	/// </param>
	/// <returns>
	/// A Vector3 containing the Euler angles of the Quaternion.
	/// </returns>
	public static Vector3 ToEulerAngle(this Quaternion quaternion, bool asDegrees)
	{
		double eax;
		double eay;
		double eaz;

		float qx = quaternion.X;
		float qy = -quaternion.Y;
		float qz = -quaternion.Z;
		float qw = quaternion.W;

		double[,] M = new double[4, 4];

		double Nq = (qx * qx) + (qy * qy) + (qz * qz) + (qw * qw);
		double s = Nq > 0.0 ? 2.0 / Nq : 0.0;
		double xs = qx * s, ys = qy * s, zs = qz * s;
		double wx = qw * xs, wy = qw * ys, wz = qw * zs;
		double xx = qx * xs, xy = qx * ys, xz = qx * zs;
		double yy = qy * ys, yz = qy * zs, zz = qz * zs;

		M[0, 0] = 1.0 - (yy + zz); M[0, 1] = xy - wz; M[0, 2] = xz + wy;
		M[1, 0] = xy + wz; M[1, 1] = 1.0 - (xx + zz); M[1, 2] = yz - wx;
		M[2, 0] = xz - wy; M[2, 1] = yz + wx; M[2, 2] = 1.0 - (xx + yy);
		M[3, 0] = M[3, 1] = M[3, 2] = M[0, 3] = M[1, 3] = M[2, 3] = 0.0; M[3, 3] = 1.0;

		double test = Math.Sqrt((M[0, 0] * M[0, 0]) + (M[1, 0] * M[1, 0]));
		if (test > 16 * 1.19209290E-07F)//FLT_EPSILON
		{
			eax = Math.Atan2(M[2, 1], M[2, 2]);
			eay = Math.Atan2(-M[2, 0], test);
			eaz = Math.Atan2(M[1, 0], M[0, 0]);
		}
		else
		{
			eax = Math.Atan2(-M[1, 2], M[1, 1]);
			eay = Math.Atan2(-M[2, 0], test);
			eaz = 0;
		}

		return new()
		{
			X = GetAngle(eax, asDegrees),
			Y = GetAngle(eay, asDegrees),
			Z = GetAngle(eaz, asDegrees),
		};
	}

	private static float GetAngle(double radians, bool convertToDegrees)
	{
		return (float)(convertToDegrees ? RadiansToDegrees(radians) : radians);
	}

	private static double RadiansToDegrees(double radians)
	{
		return radians * 180.0 / Math.PI;
	}

	/// <summary>
	/// Returns the dot product of two Quaternions.
	/// </summary>
	/// <param name="a">The first Quaternion.</param>
	/// <param name="b">The second Quaternion.</param>
	/// <returns>The dot product of two Quaternions.</returns>
	public static double Dot(this Quaternion a, Quaternion b)
	{
		return ((double)a.X * b.X) + ((double)a.Y * b.Y) + ((double)a.Z * b.Z) + ((double)a.W * b.W);
	}

	/// <summary>
	/// Returns a value indicating if this Quaternion is an unit quaternion.
	/// </summary>
	/// <param name="a">The Quaternion instance.</param>
	/// <returns>
	/// True if this Quaternion is an unit quaternion.
	/// </returns>
	public static bool IsUnitQuaternion(this Quaternion a)
	{
		return ((a.X * a.X) + (a.Y * a.Y) + (a.Z * a.Z) + (a.W * a.W)) > 1d - kEpsilon;
	}

	/// <summary>
	/// Returns a value indicating if this Quaternion is the zero Quaternion.
	/// </summary>
	/// <param name="a">The Quaternion instance.</param>
	/// <returns>True if this Quaternion is the zero Quaternion.</returns>
	public static bool IsZero(this Quaternion a)
	{
		return a.X == 0 && a.Y == 0 && a.Z == 0 && a.W == 0;
	}

	/// <summary>
	/// Returns a value indicating if this Quaternion is nearly equal to another using dot product.
	/// </summary>
	/// <param name="a">The first Quaternion.</param>
	/// <param name="b">The second Quaternion.</param>
	/// <returns>True if they are nearly equal.</returns>
	public static bool EqualsByDot(this Quaternion a, Quaternion b)
	{
		return a.Dot(b) > 1.0f - kEpsilon;
	}

	private const float kEpsilon = 0.00001F;
}
