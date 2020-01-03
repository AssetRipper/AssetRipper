using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Layout;

namespace uTinyRipper.Classes
{
	public struct Vector4f : IAsset
	{
		public Vector4f(float value) :
			this(value, value, value, value)
		{
		}

		public Vector4f(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public static implicit operator Vector4f(Vector2f v2) => new Vector4f(v2.X, v2.Y, 0.0f, 0.0f);
		public static implicit operator Vector4f(Vector2i v2) => new Vector4f(v2.X, v2.Y, 0.0f, 0.0f);
		public static implicit operator Vector4f(Vector3f v3) => new Vector4f(v3.X, v3.Y, v3.Z, 0.0f);
		public static implicit operator Vector4f(Vector3i v3) => new Vector4f(v3.X, v3.Y, v3.Z, 0.0f);

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

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode(MappingStyle.Flow);
			Vector4fLayout layout = container.ExportLayout.Serialized.Vector4f;
			node.Add(layout.XName, X);
			node.Add(layout.YName, Y);
			node.Add(layout.ZName, Z);
			node.Add(layout.WName, W);
			return node;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType() != typeof(Vector4f))
			{
				return false;
			}
			return this == (Vector4f)obj;
		}

		public override int GetHashCode()
		{
			int hash = 1187;
			unchecked
			{
				hash = hash + 617 * X.GetHashCode();
				hash = hash * 277 + Y.GetHashCode();
				hash = hash * 349 + Z.GetHashCode();
				hash = hash * 223 + W.GetHashCode();
			}
			return hash;
		}

		public override string ToString()
		{
			return $"[{X:0.00}, {Y:0.00}, {Z:0.00}, {W:0.00}]";
		}

		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }
		public float W { get; set; }
	}
}
