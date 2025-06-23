namespace AssetRipper.Numerics;

public static class Vector3Extensions
{
	public static Quaternion ToQuaternion(this Vector3 source, bool isDegrees)
	{
		// Abbreviations for the various angular functions
		double cy = Math.Cos(GetRadians(source.Z, isDegrees) * 0.5);
		double sy = Math.Sin(GetRadians(source.Z, isDegrees) * 0.5);
		double cp = Math.Cos(GetRadians(source.Y, isDegrees) * 0.5);
		double sp = Math.Sin(GetRadians(source.Y, isDegrees) * 0.5);
		double cr = Math.Cos(GetRadians(source.X, isDegrees) * 0.5);
		double sr = Math.Sin(GetRadians(source.X, isDegrees) * 0.5);

		return new()
		{
			W = -(float)((cr * cp * cy) + (sr * sp * sy)),
			X = -(float)((sr * cp * cy) - (cr * sp * sy)),
			Y = (float)((cr * sp * cy) + (sr * cp * sy)),
			Z = (float)((cr * cp * sy) - (sr * sp * cy)),
		};
	}

	private static double GetRadians(double angle, bool isDegrees)
	{
		return isDegrees ? (angle * Math.PI / 180.0) : angle;
	}

	public static void Normalize(this Vector3 instance)
	{
		float length = instance.Length();
		if (length > kEpsilon)
		{
			instance.X /= length;
			instance.Y /= length;
			instance.Z /= length;
		}
		else
		{
			instance.X = 0;
			instance.Y = 0;
			instance.Z = 0;
		}
	}

	public static float Dot(this Vector3 instance, Vector3 other)
	{
		return (instance.X * other.X) + (instance.Y * other.Y) + (instance.Z * other.Z);
	}

	public static bool EqualsByDot(this Vector3 instance, Vector3 other)
	{
		float instanceLength = instance.Length();
		float otherLength = other.Length();

		if (instanceLength < kEpsilon)
		{
			return otherLength < kEpsilon;
		}

		if (otherLength < kEpsilon)
		{
			return false;
		}

		float dot = instance.Dot(other);
		float deviation = 1f - (dot / instanceLength / otherLength);
		return deviation < kEpsilon;
	}

	private const float kEpsilon = 0.00001F;
}
