using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Globalization;

namespace AssetRipper.Core.Math.Vectors
{
	public sealed class Quaternionf : IAsset, IEquatable<Quaternionf>, IQuaternionf
	{
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }
		public float W { get; set; }
		public const string XName = "x";
		public const string YName = "y";
		public const string ZName = "z";
		public const string WName = "w";

		public Quaternionf() { }

		public Quaternionf(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
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
			YAMLMappingNode node = new YAMLMappingNode();
			node.Style = MappingStyle.Flow;
			node.Add(XName, X);
			node.Add(YName, Y);
			node.Add(ZName, Z);
			node.Add(WName, W);
			return node;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "[{0:0.00}, {1:0.00}, {2:0.00}, {3:0.00}]", X, Y, Z, W);
		}

		public static Quaternionf Zero => new Quaternionf(0.0f, 0.0f, 0.0f, 1.0f);

		public static implicit operator Quaternionf(Vector4f v) => new Vector4f(v.X, v.Y, v.Z, v.W);

		public static implicit operator System.Numerics.Quaternion(Quaternionf q) => new System.Numerics.Quaternion(q.X, q.Y, q.Z, q.W);
		public static implicit operator Quaternionf(System.Numerics.Quaternion q) => new Quaternionf(q.X, q.Y, q.Z, q.W);

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode() << 2 ^ Z.GetHashCode() >> 2 ^ W.GetHashCode() >> 1;
		}

		public override bool Equals(object other)
		{
			if (other is Quaternionf quat)
				return Equals(quat);
			else
				return false;
		}

		public bool Equals(Quaternionf other)
		{
			return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
		}

		public static bool operator ==(Quaternionf lhs, Quaternionf rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(Quaternionf lhs, Quaternionf rhs)
		{
			return !(lhs == rhs);
		}
	}
}
