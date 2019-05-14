using System;
using System.Collections.Generic;
using System.Globalization;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public struct Quaternionf : ISerializableStructure
	{
		public Quaternionf(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public float this[int index]
		{
			get
			{
				switch(index)
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

		public ISerializableStructure CreateDuplicate()
		{
			return new Quaternionf();
		}

		public void Read(AssetReader reader)
		{
			X = reader.ReadSingle();
			Y = reader.ReadSingle();
			Z = reader.ReadSingle();
			W = reader.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Style = MappingStyle.Flow;
			node.Add("x", X);
			node.Add("y", Y);
			node.Add("z", Z);
			node.Add("w", W);
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
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

			double test = Math.Sqrt(M[0, 0] * M[0, 0] + M[1, 0] * M[1, 0]);
			if (test > 16 * 1.19209290E-07F)//FLT_EPSILON
			{
				eax = Math.Atan2(M[2, 1], M[2, 2]);
				eay = Math.Atan2(-M[2, 0], test);
				eaz = Math.Atan2(M[1, 0], M[0, 0]);
			}
			else
			{
				eax = Math.Atan2(-M[1, 2], M[1, 1]);
				eay = Math.Atan2(-M[2, 0], test);
				eaz = 0;
			}

			float x = (float)(eax * 180.0 / Math.PI);
			float y = (float)(eay * 180.0 / Math.PI);
			float z = (float)(eaz * 180.0 / Math.PI);
			Vector3f euler = new Vector3f(x, y, z);
			return euler;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "[{0:0.00}, {1:0.00}, {2:0.00}, {3:0.00}]", X, Y, Z, W);
		}

		public static Quaternionf Zero => new Quaternionf(0.0f, 0.0f, 0.0f, 1.0f);
		public static Quaternionf DefaultWeight => new Quaternionf(1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f);

		public float X { get; private set; }
		public float Y { get; private set; }
		public float Z { get; private set; }
		public float W { get; private set; }
	}
}
