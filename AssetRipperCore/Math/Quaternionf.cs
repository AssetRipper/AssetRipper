using AssetRipper.Core.Project;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.YAML;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace AssetRipper.Core.Math
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct Quaternionf : IAsset, IEquatable<Quaternionf>
	{
		public float X;
		public float Y;
		public float Z;
		public float W;

		public Quaternionf(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public Vector3f ToEuler()
		{
			double eax;
			double eay;
			double eaz;

			float qx = X;
			float qy = -Y;
			float qz = -Z;
			float qw = W;

			double[,] M = new double[4, 4];

			double Nq = qx * qx + qy * qy + qz * qz + qw * qw;
			double s = (Nq > 0.0) ? (2.0 / Nq) : 0.0;
			double xs = qx * s, ys = qy * s, zs = qz * s;
			double wx = qw * xs, wy = qw * ys, wz = qw * zs;
			double xx = qx * xs, xy = qx * ys, xz = qx * zs;
			double yy = qy * ys, yz = qy * zs, zz = qz * zs;

			M[0, 0] = 1.0 - (yy + zz); M[0, 1] = xy - wz; M[0, 2] = xz + wy;
			M[1, 0] = xy + wz; M[1, 1] = 1.0 - (xx + zz); M[1, 2] = yz - wx;
			M[2, 0] = xz - wy; M[2, 1] = yz + wx; M[2, 2] = 1.0 - (xx + yy);
			M[3, 0] = M[3, 1] = M[3, 2] = M[0, 3] = M[1, 3] = M[2, 3] = 0.0; M[3, 3] = 1.0;

			double test = System.Math.Sqrt(M[0, 0] * M[0, 0] + M[1, 0] * M[1, 0]);
			if (test > 16 * 1.19209290E-07F)//FLT_EPSILON
			{
				eax = System.Math.Atan2(M[2, 1], M[2, 2]);
				eay = System.Math.Atan2(-M[2, 0], test);
				eaz = System.Math.Atan2(M[1, 0], M[0, 0]);
			}
			else
			{
				eax = System.Math.Atan2(-M[1, 2], M[1, 1]);
				eay = System.Math.Atan2(-M[2, 0], test);
				eaz = 0;
			}

			float x = (float)(eax * 180.0 / System.Math.PI);
			float y = (float)(eay * 180.0 / System.Math.PI);
			float z = (float)(eaz * 180.0 / System.Math.PI);
			Vector3f euler = new Vector3f(x, y, z);
			return euler;
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
			QuaternionfLayout layout = container.ExportLayout.Serialized.Quaternionf;
			node.Style = MappingStyle.Flow;
			node.Add(layout.XName, X);
			node.Add(layout.YName, Y);
			node.Add(layout.ZName, Z);
			node.Add(layout.WName, W);
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

		public float this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return X;
					case 1:
						return Y;
					case 2:
						return Z;
					case 3:
						return W;

					default:
						throw new IndexOutOfRangeException($"Index {index} is out of bound");
				}
			}
			set
			{
				switch (index)
				{
					case 0:
						X = value;
						break;
					case 1:
						Y = value;
						break;
					case 2:
						Z = value;
						break;
					case 3:
						W = value;
						break;

					default:
						throw new IndexOutOfRangeException($"Index {index} is out of bound");
				}
			}
		}

		public static float Dot(Quaternionf a, Quaternionf b)
		{
			return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
		}

		private static bool IsEqualUsingDot(float dot)
		{
			return dot > 1.0f - kEpsilon;
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ (Y.GetHashCode() << 2) ^ (Z.GetHashCode() >> 2) ^ (W.GetHashCode() >> 1);
		}

		public override bool Equals(object other)
		{
			if (!(other is Quaternionf))
				return false;
			return Equals((Quaternionf)other);
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

		private const float kEpsilon = 0.000001F;
	}
}
