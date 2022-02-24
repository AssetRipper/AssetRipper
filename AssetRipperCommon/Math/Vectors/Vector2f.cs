using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Globalization;

namespace AssetRipper.Core.Math.Vectors
{
	public sealed class Vector2f : IAsset, IEquatable<Vector2f>, IVector2f
	{
		public float X { get; set; }
		public float Y { get; set; }

		public Vector2f() { }

		public Vector2f(float value) : this(value, value) { }

		public Vector2f(float x, float y)
		{
			X = x;
			Y = y;
		}

		public static implicit operator Vector2f(Vector2i v2) => v2 is null ? new() : new Vector2f(v2.X, v2.Y);
		public static explicit operator Vector2f(Vector3f v3) => new Vector2f(v3.X, v3.Y);
		public static explicit operator Vector2f(Vector3i v3) => v3 is null ? new() : new Vector2f(v3.X, v3.Y);

		public static implicit operator System.Numerics.Vector2(Vector2f v2) => new System.Numerics.Vector2(v2.X, v2.Y);
		public static implicit operator Vector2f(System.Numerics.Vector2 v2) => new Vector2f(v2.X, v2.Y);

		public float this[int index]
		{
			get
			{
				return index switch
				{
					0 => X,
					1 => Y,
					_ => throw new ArgumentOutOfRangeException(nameof(index), "Invalid Vector2f index!"),
				};
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
			node.Style = MappingStyle.Flow;
			node.Add(XName, X);
			node.Add(YName, Y);
			return node;
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode() << 2;
		}

		public override bool Equals(object other)
		{
			if (other is Vector2f asset)
				return Equals(asset);
			else
				return false;
		}

		public bool Equals(Vector2f other)
		{
			return X == other.X && Y == other.Y;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "[{0:0.00}, {1:0.00}]", X, Y);
		}

		public static Vector2f One { get; } = new Vector2f(1.0f, 1.0f);

		public static Vector2f Zero { get; } = new Vector2f();

		public const string XName = "x";
		public const string YName = "y";
	}
}
