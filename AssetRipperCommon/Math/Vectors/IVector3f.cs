using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Math.Vectors
{
	public interface IVector3f : IAsset
	{
		float X { get; set; }
		float Y { get; set; }
		float Z { get; set; }
	}

	public static class Vector3fExtensions
	{
		public static void CopyValuesFrom(this IVector3f instance, IVector3f source)
		{
			instance.X = source.X;
			instance.Y = source.Y;
			instance.Z = source.Z;
		}

		public static void Reset(this IVector3f instance)
		{
			instance.X = 0;
			instance.Y = 0;
			instance.Z = 0;
		}

		public static Quaternionf ToQuaternion(this IVector3f source, bool isDegrees = true)
		{
			Quaternionf result = new Quaternionf();
			source.ToQuaternion(result, isDegrees);
			return result;
		}

		public static void ToQuaternion(this IVector3f source, IQuaternionf destination, bool isDegrees)
		{
			// Abbreviations for the various angular functions
			double cy = System.Math.Cos(GetRadians(source.Z, isDegrees) * 0.5);
			double sy = System.Math.Sin(GetRadians(source.Z, isDegrees) * 0.5);
			double cp = System.Math.Cos(GetRadians(source.Y, isDegrees) * 0.5);
			double sp = System.Math.Sin(GetRadians(source.Y, isDegrees) * 0.5);
			double cr = System.Math.Cos(GetRadians(source.X, isDegrees) * 0.5);
			double sr = System.Math.Sin(GetRadians(source.X, isDegrees) * 0.5);

			destination.W = -(float)(cr * cp * cy + sr * sp * sy);
			destination.X = -(float)(sr * cp * cy - cr * sp * sy);
			destination.Y = (float)(cr * sp * cy + sr * cp * sy);
			destination.Z = (float)(cr * cp * sy - sr * sp * cy);
		}

		private static double GetRadians(double angle, bool isDegrees)
		{
			return isDegrees ? (angle * System.Math.PI / 180.0) : angle;
		}

		public static void Normalize(this IVector3f instance)
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

		public static float Length(this IVector3f instance)
		{
			return (float)System.Math.Sqrt(instance.LengthSquared());
		}

		public static float LengthSquared(this IVector3f instance)
		{
			return instance.X * instance.X + instance.Y * instance.Y + instance.Z * instance.Z;
		}

		public static float Dot(this IVector3f instance, IVector3f other)
		{
			return instance.X * other.X + instance.Y * other.Y + instance.Z * other.Z;
		}

		public static bool IsEqualByDot(this IVector3f instance, IVector3f other)
		{
			float instanceLength = instance.Length();
			float otherLength = other.Length();

			if(instanceLength < kEpsilon)
				return otherLength < kEpsilon;
			if (otherLength < kEpsilon)
				return false;
			
			float dot = instance.Dot(other);
			float deviation = 1f - ((dot / instanceLength) / otherLength);
			return deviation < kEpsilon;
		}

		private const float kEpsilon = 0.00001F;
	}
}
