using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Math.Vectors
{
	public interface IVector2f : IAsset
	{
		float X { get; set; }
		float Y { get; set; }
	}

	public static class Vector2fExtensions
	{
		public static void Normalize(this IVector2f vector)
		{
			double length = vector.Length();
			if (length > kEpsilon)
			{
				vector.X = (float)(vector.X / length);
				vector.Y = (float)(vector.Y / length);
			}
			else
			{
				vector.X = 0;
				vector.Y = 0;
			}
		}

		public static double Length(this IVector2f vector)
		{
			return System.Math.Sqrt(vector.LengthSquared());
		}

		public static double LengthSquared(this IVector2f vector)
		{
			return (double)vector.X * vector.X + (double)vector.Y * vector.Y;
		}

		private const float kEpsilon = 0.00001F;
	}
}
