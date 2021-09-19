using AssetRipper.Core.Project;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.YAML;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace AssetRipper.Core.Math
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct Vector2f : IAsset, IEquatable<Vector2f>
	{
		public float X;
		public float Y;

		public Vector2f(float value) : this(value, value) { }

		public Vector2f(float x, float y)
		{
			X = x;
			Y = y;
		}

		public static implicit operator Vector2f(Vector2i v2) => new Vector2f(v2.X, v2.Y);
		public static explicit operator Vector2f(Vector3f v3) => new Vector2f(v3.X, v3.Y);
		public static explicit operator Vector2f(Vector3i v3) => new Vector2f(v3.X, v3.Y);

		public static implicit operator System.Numerics.Vector2(Vector2f v2) => new System.Numerics.Vector2(v2.X, v2.Y);
		public static implicit operator Vector2f(System.Numerics.Vector2 v2) => new Vector2f(v2.X, v2.Y);

		public float this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return X;
					case 1: return Y;
					default: throw new ArgumentOutOfRangeException(nameof(index), "Invalid Vector2f index!");
				}
			}

			set
			{
				switch (index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					default: throw new ArgumentOutOfRangeException(nameof(index), "Invalid Vector2f index!");
				}
			}
		}

		public static Vector2f operator -(Vector2f left)
		{
			return new Vector2f(-left.X, -left.Y);
		}

		public static Vector2f operator -(Vector2f left, Vector2f right)
		{
			return new Vector2f(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2f operator +(Vector2f left, Vector2f right)
		{
			return new Vector2f(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2f operator *(Vector2f left, float right)
		{
			return new Vector2f(left.X * right, left.Y * right);
		}

		public static Vector2f operator /(Vector2f left, float right)
		{
			return new Vector2f(left.X / right, left.Y / right);
		}

		public static bool operator ==(Vector2f left, Vector2f right)
		{
			return left.X == right.X && left.Y == right.Y;
		}

		public static bool operator !=(Vector2f left, Vector2f right)
		{
			return left.X != right.X || left.Y != right.Y;
		}

		public static Vector2f Scale(Vector2f left, Vector2f right)
		{
			return new Vector2f(left.X * right.X, left.Y * right.Y);
		}

		/// <summary>
		/// Angle increase when 2nd line is moving in clockwise direction
		/// </summary>
		/// <returns>Angle in degrees</returns>
		public static float AngleFrom3Points(Vector2f point1, Vector2f point2, Vector2f point3)
		{
			Vector2f transformedP1 = new Vector2f(point1.X - point2.X, point1.Y - point2.Y);
			Vector2f transformedP2 = new Vector2f(point3.X - point2.X, point3.Y - point2.Y);

			double angleToP1 = System.Math.Atan2(transformedP1.Y, transformedP1.X);
			double angleToP2 = System.Math.Atan2(transformedP2.Y, transformedP2.X);

			double angle = angleToP1 - angleToP2;
			if (angle < 0)
			{
				angle += (2 * System.Math.PI);
			}
			return (float)(360.0 * angle / (2.0 * System.Math.PI));
		}

		public void Read(AssetReader reader)
		{
			X = reader.ReadSingle();
			Y = reader.ReadSingle();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			Vector2fLayout layout = container.ExportLayout.Serialized.Vector2f;
			node.Style = MappingStyle.Flow;
			node.Add(layout.XName, X);
			node.Add(layout.YName, Y);
			return node;
		}

		public void Normalize()
		{
			var length = Length();
			if (length > kEpsilon)
			{
				var invNorm = 1.0f / length;
				X *= invNorm;
				Y *= invNorm;
			}
			else
			{
				X = 0;
				Y = 0;
			}
		}

		public float Length()
		{
			return (float)System.Math.Sqrt(LengthSquared());
		}

		public float LengthSquared()
		{
			return X * X + Y * Y;
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ (Y.GetHashCode() << 2);
		}

		public override bool Equals(object other)
		{
			if (!(other is Vector2f))
				return false;
			return Equals((Vector2f)other);
		}

		public bool Equals(Vector2f other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y);
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "[{0:0.00}, {1:0.00}]", X, Y);
		}

		public static Vector2f One { get; } = new Vector2f(1.0f, 1.0f);

		public static Vector2f Zero { get; } = new Vector2f();

		private const float kEpsilon = 0.00001F;
	}
}
