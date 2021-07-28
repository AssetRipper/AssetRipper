using System;
using System.Runtime.InteropServices;

namespace AssetRipper.Math
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct Color : IEquatable<Color>
	{
		public float R;
		public float G;
		public float B;
		public float A;

		public Color(float r, float g, float b, float a)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public override int GetHashCode()
		{
			return ((Vector4f)this).GetHashCode();
		}

		public override bool Equals(object other)
		{
			if (!(other is Color))
				return false;
			return Equals((Color)other);
		}

		public bool Equals(Color other)
		{
			return R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A);
		}

		public static Color operator +(Color a, Color b)
		{
			return new Color(a.R + b.R, a.G + b.G, a.B + b.B, a.A + b.A);
		}

		public static Color operator -(Color a, Color b)
		{
			return new Color(a.R - b.R, a.G - b.G, a.B - b.B, a.A - b.A);
		}

		public static Color operator *(Color a, Color b)
		{
			return new Color(a.R * b.R, a.G * b.G, a.B * b.B, a.A * b.A);
		}

		public static Color operator *(Color a, float b)
		{
			return new Color(a.R * b, a.G * b, a.B * b, a.A * b);
		}

		public static Color operator *(float b, Color a)
		{
			return new Color(a.R * b, a.G * b, a.B * b, a.A * b);
		}

		public static Color operator /(Color a, float b)
		{
			return new Color(a.R / b, a.G / b, a.B / b, a.A / b);
		}

		public static bool operator ==(Color lhs, Color rhs)
		{
			return (Vector4f)lhs == (Vector4f)rhs;
		}

		public static bool operator !=(Color lhs, Color rhs)
		{
			return !(lhs == rhs);
		}

		public static implicit operator Vector4f(Color c)
		{
			return new Vector4f(c.R, c.G, c.B, c.A);
		}
	}
}
