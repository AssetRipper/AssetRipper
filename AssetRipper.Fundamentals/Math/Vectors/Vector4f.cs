using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Math.Vectors
{
	public sealed class Vector4f : IAsset, IEquatable<Vector4f>, IVector4f
	{
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }
		public float W { get; set; }
		public const string XName = "x";
		public const string YName = "y";
		public const string ZName = "z";
		public const string WName = "w";

		public Vector4f() { }

		public Vector4f(float value) : this(value, value, value, value) { }

		public Vector4f(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public Vector4f(Vector3f value, float w)
		{
			X = value.X;
			Y = value.Y;
			Z = value.Z;
			W = w;
		}

		public float this[int index]
		{
			get
			{
				return index switch
				{
					0 => X,
					1 => Y,
					2 => Z,
					3 => W,
					_ => throw new ArgumentOutOfRangeException(nameof(index), "Invalid Vector4 index!"),
				};
			}

			set
			{
				switch (index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
					case 3: W = value; break;
					default: throw new ArgumentOutOfRangeException(nameof(index), "Invalid Vector4 index!");
				}
			}
		}

		public static implicit operator Vector4f(Vector2f v2) => new(v2.X, v2.Y, 0.0f, 0.0f);
		public static implicit operator Vector4f(Vector2i v2) => v2 is null ? new() : new(v2.X, v2.Y, 0.0f, 0.0f);
		public static implicit operator Vector4f(Vector3f v3) => new(v3.X, v3.Y, v3.Z, 0.0f);
		public static implicit operator Vector4f(Vector3i v3) => v3 is null ? new() : new(v3.X, v3.Y, v3.Z, 0.0f);

		public static explicit operator Vector4f(Quaternionf q) => new(q.X, q.Y, q.Z, q.W);
		public static explicit operator ColorRGBAf(Vector4f v) => new(v.X, v.Y, v.Z, v.W);

		public static implicit operator System.Numerics.Vector4(Vector4f v4) => new(v4.X, v4.Y, v4.Z, v4.W);
		public static implicit operator Vector4f(System.Numerics.Vector4 v4) => new(v4.X, v4.Y, v4.Z, v4.W);

		public static Vector4f operator -(Vector4f left)
		{
			return new Vector4f(-left.X, -left.Y, -left.Z, -left.W);
		}

		public static Vector4f operator -(Vector4f left, Vector4f right)
		{
			return new Vector4f(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Vector4f operator +(Vector4f left, Vector4f right)
		{
			return new Vector4f(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Vector4f operator *(Vector4f left, float right)
		{
			return new Vector4f(left.X * right, left.Y * right, left.Z * right, left.W * right);
		}

		public static Vector4f operator *(float left, Vector4f right)
		{
			return new Vector4f(right.X * left, right.Y * left, right.Z * left, right.W * left);
		}

		public static Vector4f operator /(Vector4f left, float right)
		{
			return new Vector4f(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}

		public static bool operator ==(Vector4f left, Vector4f right)
		{
			return left.X == right.X && left.Y == right.Y && left.Z == right.Z && left.W == right.W;
		}

		public static bool operator !=(Vector4f left, Vector4f right)
		{
			return left.X != right.X || left.Y != right.Y || left.Z != right.Z || left.W != right.W;
		}

		public void Read(AssetReader reader)
		{
			X = reader.ReadSingle();
			Y = reader.ReadSingle();
			Z = reader.ReadSingle();
			W = reader.ReadSingle();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
			writer.Write(W);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new(MappingStyle.Flow);
			node.Add(XName, X);
			node.Add(YName, Y);
			node.Add(ZName, Z);
			node.Add(WName, W);
			return node;
		}

		public override bool Equals(object? other)
		{
			if (other is Vector4f vector)
			{
				return Equals(vector);
			}
			return false;
		}

		public bool Equals(Vector4f? other)
		{
			return other is not null && X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ (Y.GetHashCode() << 2) ^ (Z.GetHashCode() >> 2) ^ (W.GetHashCode() >> 1);
		}

		public override string ToString()
		{
			return $"[{X:0.00}, {Y:0.00}, {Z:0.00}, {W:0.00}]";
		}

		public void Normalize()
		{
			float length = Length();
			if (length > kEpsilon)
			{
				float invNorm = 1.0f / length;
				X *= invNorm;
				Y *= invNorm;
				Z *= invNorm;
				W *= invNorm;
			}
			else
			{
				X = 0;
				Y = 0;
				Z = 0;
				W = 0;
			}
		}

		public float Length()
		{
			return (float)System.Math.Sqrt(LengthSquared());
		}

		public float LengthSquared()
		{
			return (X * X) + (Y * Y) + (Z * Z) + (W * W);
		}

		public static Vector4f Zero => new();

		private const float kEpsilon = 0.00001F;
	}
}
