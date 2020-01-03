using System;
using System.Globalization;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Layout;

namespace uTinyRipper.Classes
{
	public struct Vector2f : IAsset
	{
		public Vector2f(float value) :
			this(value, value)
		{
		}

		public Vector2f(float x, float y)
		{
			X = x;
			Y = y;
		}

		public static implicit operator Vector2f(Vector2i v2) => new Vector2f(v2.X, v2.Y);
		public static explicit operator Vector2f(Vector3f v3) => new Vector2f(v3.X, v3.Y);
		public static explicit operator Vector2f(Vector3i v3) => new Vector2f(v3.X, v3.Y);

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

		/// <summary>
		/// Angle increase when 2nd line is moving in clockwise direction
		/// </summary>
		/// <returns>Angle in degrees</returns>
		public static float AngleFrom3Points(Vector2f point1, Vector2f point2, Vector2f point3)
		{
			Vector2f transformedP1 = new Vector2f(point1.X - point2.X, point1.Y - point2.Y);
			Vector2f transformedP2 = new Vector2f(point3.X - point2.X, point3.Y - point2.Y);

			double angleToP1 = Math.Atan2(transformedP1.Y, transformedP1.X);
			double angleToP2 = Math.Atan2(transformedP2.Y, transformedP2.X);

			double angle = angleToP1 - angleToP2;
			if (angle < 0)
			{
				angle += (2 * Math.PI);
			}
			return (float)(360.0 * angle / (2.0 * Math.PI));
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

		public float GetMember(int index)
		{
			if (index == 0)
			{
				return X;
			}
			if (index == 1)
			{
				return Y;
			}
			throw new ArgumentException($"Invalid index {index}", nameof(index));
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType() != typeof(Vector2f))
			{
				return false;
			}
			return this == (Vector2f)obj;
		}

		public override int GetHashCode()
		{
			int hash = 61;
			unchecked
			{
				hash = hash + 977 * X.GetHashCode();
				hash = hash * 73 + Y.GetHashCode();
			}
			return hash;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "[{0:0.00}, {1:0.00}]", X, Y);
		}

		public static Vector2f One { get; } = new Vector2f(1.0f, 1.0f);

		public float X { get; set; }
		public float Y { get; set; }
	}
}
