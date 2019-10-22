using System;
using System.Collections.Generic;
using System.Globalization;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public struct Vector3f : IAsset, ISerializableStructure
	{
		public Vector3f(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static explicit operator Vector3f(Vector2f v2) => new Vector3f(v2.X, v2.Y, 0.0f);
		public static explicit operator Vector3f(Vector2i v2) => new Vector3f(v2.X, v2.Y, 0.0f);
		public static explicit operator Vector3f(Vector3i v3) => new Vector3f(v3.X, v3.Y, v3.Z);

		public static Vector3f operator -(Vector3f left)
		{
			return new Vector3f(-left.X, -left.Y, -left.Z);
		}

		public static Vector3f operator -(Vector3f left, Vector3f right)
		{
			return new Vector3f(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3f operator +(Vector3f left, Vector3f right)
		{
			return new Vector3f(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3f operator *(Vector3f left, float right)
		{
			return new Vector3f(left.X * right, left.Y * right, left.Z * right);
		}

		public static Vector3f operator /(Vector3f left, float right)
		{
			return new Vector3f(left.X / right, left.Y / right, left.Z / right);
		}

		public static bool operator ==(Vector3f left, Vector3f right)
		{
			return left.X == right.X && left.Y == right.Y && left.Z == right.Z;
		}

		public static bool operator !=(Vector3f left, Vector3f right)
		{
			return left.X != right.X || left.Y != right.Y || left.Z != right.Z;
		}

		public ISerializableStructure CreateDuplicate()
		{
			return new Vector3f();
		}

		public void Read(AssetReader reader)
		{
			X = reader.ReadSingle();
			Y = reader.ReadSingle();
			Z = reader.ReadSingle();
		}

		public void Read2(AssetReader reader)
		{
			X = reader.ReadSingle();
			Y = reader.ReadSingle();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Style = MappingStyle.Flow;
			node.Add(XName, X);
			node.Add(YName, Y);
			node.Add(ZName, Z);
			return node;
		}

		public YAMLNode ExportYAML2(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Style = MappingStyle.Flow;
			node.Add(XName, X);
			node.Add(YName, Y);
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		public Vector2f ToVector2()
		{
			return new Vector2f(X, Y);
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
			if (index == 2)
			{
				return Z;
			}
			throw new ArgumentException($"Invalid index {index}", nameof(index));
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType() != typeof(Vector3f))
			{
				return false;
			}
			return this == (Vector3f)obj;
		}

		public override int GetHashCode()
		{
			int hash = 853;
			unchecked
			{
				hash = hash + 269 * X.GetHashCode();
				hash = hash * 757 + Y.GetHashCode();
				hash = hash * 131 + Z.GetHashCode();
			}
			return hash;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "[{0:0.00}, {1:0.00}, {2:0.00}]", X, Y, Z);
		}

		public static Vector3f One => new Vector3f(1.0f, 1.0f, 1.0f);

		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }

		public const string XName = "x";
		public const string YName = "y";
		public const string ZName = "z";
	}
}
