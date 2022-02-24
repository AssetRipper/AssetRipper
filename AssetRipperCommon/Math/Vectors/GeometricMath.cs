using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetRipper.Core.Math.Vectors
{
	public static class GeometricMath
	{
		private const float kEpsilon = 0.00001F;

		/// <summary>
		/// Angle increase when 2nd line is moving in clockwise direction
		/// </summary>
		/// <returns>Angle in degrees</returns>
		public static float AngleFrom3Points(IVector2f point1, IVector2f point2, IVector2f point3)
		{
			float transformedP1X = point1.X - point2.X;
			float transformedP1Y = point1.Y - point2.Y;
			float transformedP2X = point3.X - point2.X;
			float transformedP2Y = point3.Y - point2.Y;

			double angleToP1 = System.Math.Atan2(transformedP1Y, transformedP1X);
			double angleToP2 = System.Math.Atan2(transformedP2Y, transformedP2X);

			double angle = angleToP1 - angleToP2;
			if (angle < 0)
				angle += 2 * System.Math.PI;
			return (float)(360.0 * angle / (2.0 * System.Math.PI));
		}
	}
}
