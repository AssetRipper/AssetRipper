using System.Globalization;
using System.Numerics;

namespace AssetRipper.Core.Math.Colors
{
	public readonly record struct ColorFloat(Vector4 Vector)
	{
		public ColorFloat(float R, float G, float B, float A) : this(new Vector4(R, G, B, A)) { }
		
		public float R => Vector.X;
		public float G => Vector.Y;
		public float B => Vector.Z;
		public float A => Vector.W;
		
		public ColorFloat Clamp()
		{
			return new ColorFloat(
				System.Math.Min(System.Math.Max(R, 0), 1),
				System.Math.Min(System.Math.Max(G, 0), 1),
				System.Math.Min(System.Math.Max(B, 0), 1),
				System.Math.Min(System.Math.Max(A, 0), 1)
			);
		}

		public static ColorFloat operator +(ColorFloat a, ColorFloat b)
		{
			return new ColorFloat(a.Vector + b.Vector);
		}

		public static ColorFloat operator -(ColorFloat a, ColorFloat b)
		{
			return new ColorFloat(a.Vector - b.Vector);
		}

		public static ColorFloat operator *(ColorFloat a, float b)
		{
			return new ColorFloat(a.Vector * b);
		}

		public static ColorFloat operator *(float b, ColorFloat a)
		{
			return new ColorFloat(a.Vector * b);
		}

		public static ColorFloat operator /(ColorFloat a, float b)
		{
			return new ColorFloat(a.Vector / b);
		}

		public static ColorFloat Black => new ColorFloat(0, 0, 0, 1);
		public static ColorFloat White => new ColorFloat(1, 1, 1, 1);
		
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "[R:{0:0.00} G:{1:0.00} B:{2:0.00} A:{3:0.00}]", R, G, B, A);
		}
	}
}
