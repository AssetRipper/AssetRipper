using AssetRipper.Project;
using AssetRipper.Layout.Classes.Misc.Serializable;
using AssetRipper.IO.Asset;
using AssetRipper.YAML;
using System.Runtime.InteropServices;
using System;

namespace AssetRipper.Math
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct Matrix4x4f : IAsset, IEquatable<Matrix4x4f>
	{
		public float E00;
		public float E01;
		public float E02;
		public float E03;

		public float E10;
		public float E11;
		public float E12;
		public float E13;

		public float E20;
		public float E21;
		public float E22;
		public float E23;

		public float E30;
		public float E31;
		public float E32;
		public float E33;

		public Matrix4x4f(float[] values)
		{
			if (values == null)
				throw new ArgumentNullException(nameof(values));
			if (values.Length != 16)
				throw new ArgumentOutOfRangeException(nameof(values), "There must be exactly sixteen input values for Matrix.");

			E00 = values[0];
			E01 = values[1];
			E02 = values[2];
			E03 = values[3];

			E10 = values[4];
			E11 = values[5];
			E12 = values[6];
			E13 = values[7];

			E20 = values[8];
			E21 = values[9];
			E22 = values[10];
			E23 = values[11];

			E30 = values[12];
			E31 = values[13];
			E32 = values[14];
			E33 = values[15];
		}

		public void Read(AssetReader reader)
		{
			E00 = reader.ReadSingle();
			E01 = reader.ReadSingle();
			E02 = reader.ReadSingle();
			E03 = reader.ReadSingle();
			E10 = reader.ReadSingle();
			E11 = reader.ReadSingle();
			E12 = reader.ReadSingle();
			E13 = reader.ReadSingle();
			E20 = reader.ReadSingle();
			E21 = reader.ReadSingle();
			E22 = reader.ReadSingle();
			E23 = reader.ReadSingle();
			E30 = reader.ReadSingle();
			E31 = reader.ReadSingle();
			E32 = reader.ReadSingle();
			E33 = reader.ReadSingle();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(E00);
			writer.Write(E01);
			writer.Write(E02);
			writer.Write(E03);
			writer.Write(E10);
			writer.Write(E11);
			writer.Write(E12);
			writer.Write(E13);
			writer.Write(E20);
			writer.Write(E21);
			writer.Write(E22);
			writer.Write(E23);
			writer.Write(E30);
			writer.Write(E31);
			writer.Write(E32);
			writer.Write(E33);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			Matrix4x4fLayout layout = container.ExportLayout.Serialized.Matrix4x4f;
			node.Add(layout.E00Name, E00);
			node.Add(layout.E01Name, E01);
			node.Add(layout.E02Name, E02);
			node.Add(layout.E03Name, E03);
			node.Add(layout.E10Name, E10);
			node.Add(layout.E11Name, E11);
			node.Add(layout.E12Name, E12);
			node.Add(layout.E13Name, E13);
			node.Add(layout.E20Name, E20);
			node.Add(layout.E21Name, E21);
			node.Add(layout.E22Name, E22);
			node.Add(layout.E23Name, E23);
			node.Add(layout.E30Name, E30);
			node.Add(layout.E31Name, E31);
			node.Add(layout.E32Name, E32);
			node.Add(layout.E33Name, E33);
			return node;
		}

		public float this[int row, int column]
		{
			get => this[row + column * 4];

			set => this[row + column * 4] = value;
		}

		public float this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return E00;
					case 1: return E01;
					case 2: return E02;
					case 3: return E03;
					case 4: return E10;
					case 5: return E11;
					case 6: return E12;
					case 7: return E13;
					case 8: return E20;
					case 9: return E21;
					case 10: return E22;
					case 11: return E23;
					case 12: return E30;
					case 13: return E31;
					case 14: return E32;
					case 15: return E33;
					default: throw new ArgumentOutOfRangeException(nameof(index), "Invalid Matrix4x4 index!");
				}
			}

			set
			{
				switch (index)
				{
					case 0: E00 = value; break;
					case 1: E01 = value; break;
					case 2: E02 = value; break;
					case 3: E03 = value; break;
					case 4: E10 = value; break;
					case 5: E11 = value; break;
					case 6: E12 = value; break;
					case 7: E13 = value; break;
					case 8: E20 = value; break;
					case 9: E21 = value; break;
					case 10: E22 = value; break;
					case 11: E23 = value; break;
					case 12: E30 = value; break;
					case 13: E31 = value; break;
					case 14: E32 = value; break;
					case 15: E33 = value; break;
					default: throw new ArgumentOutOfRangeException(nameof(index), "Invalid Matrix4x4 index!");
				}
			}
		}

		public static Matrix4x4f Identity => new Matrix4x4f { E00 = 1.0f, E11 = 1.0f, E22 = 1.0f, E33 = 1.0f };

		public override int GetHashCode()
		{
			return GetColumn(0).GetHashCode() ^ (GetColumn(1).GetHashCode() << 2) ^ (GetColumn(2).GetHashCode() >> 2) ^ (GetColumn(3).GetHashCode() >> 1);
		}

		public override bool Equals(object other)
		{
			if (!(other is Matrix4x4f))
				return false;
			return Equals((Matrix4x4f)other);
		}

		public bool Equals(Matrix4x4f other)
		{
			return GetColumn(0).Equals(other.GetColumn(0))
				   && GetColumn(1).Equals(other.GetColumn(1))
				   && GetColumn(2).Equals(other.GetColumn(2))
				   && GetColumn(3).Equals(other.GetColumn(3));
		}

		public Vector4f GetColumn(int index)
		{
			switch (index)
			{
				case 0: return new Vector4f(E00, E01, E02, E03);
				case 1: return new Vector4f(E10, E11, E12, E13);
				case 2: return new Vector4f(E20, E21, E22, E23);
				case 3: return new Vector4f(E30, E31, E32, E33);
				default: throw new IndexOutOfRangeException("Invalid column index!");
			}
		}

		public Vector4f GetRow(int index)
		{
			switch (index)
			{
				case 0: return new Vector4f(E00, E10, E20, E30);
				case 1: return new Vector4f(E01, E11, E21, E31);
				case 2: return new Vector4f(E02, E12, E22, E32);
				case 3: return new Vector4f(E03, E13, E23, E33);
				default: throw new IndexOutOfRangeException("Invalid row index!");
			}
		}

		public static Matrix4x4f operator *(Matrix4x4f lhs, Matrix4x4f rhs)
		{
			Matrix4x4f res;
			res.E00 = lhs.E00 * rhs.E00 + lhs.E10 * rhs.E01 + lhs.E20 * rhs.E02 + lhs.E30 * rhs.E03;
			res.E10 = lhs.E00 * rhs.E10 + lhs.E10 * rhs.E11 + lhs.E20 * rhs.E12 + lhs.E30 * rhs.E13;
			res.E20 = lhs.E00 * rhs.E20 + lhs.E10 * rhs.E21 + lhs.E20 * rhs.E22 + lhs.E30 * rhs.E23;
			res.E30 = lhs.E00 * rhs.E30 + lhs.E10 * rhs.E31 + lhs.E20 * rhs.E32 + lhs.E30 * rhs.E33;

			res.E01 = lhs.E01 * rhs.E00 + lhs.E11 * rhs.E01 + lhs.E21 * rhs.E02 + lhs.E31 * rhs.E03;
			res.E11 = lhs.E01 * rhs.E10 + lhs.E11 * rhs.E11 + lhs.E21 * rhs.E12 + lhs.E31 * rhs.E13;
			res.E21 = lhs.E01 * rhs.E20 + lhs.E11 * rhs.E21 + lhs.E21 * rhs.E22 + lhs.E31 * rhs.E23;
			res.E31 = lhs.E01 * rhs.E30 + lhs.E11 * rhs.E31 + lhs.E21 * rhs.E32 + lhs.E31 * rhs.E33;

			res.E02 = lhs.E02 * rhs.E00 + lhs.E12 * rhs.E01 + lhs.E22 * rhs.E02 + lhs.E32 * rhs.E03;
			res.E12 = lhs.E02 * rhs.E10 + lhs.E12 * rhs.E11 + lhs.E22 * rhs.E12 + lhs.E32 * rhs.E13;
			res.E22 = lhs.E02 * rhs.E20 + lhs.E12 * rhs.E21 + lhs.E22 * rhs.E22 + lhs.E32 * rhs.E23;
			res.E32 = lhs.E02 * rhs.E30 + lhs.E12 * rhs.E31 + lhs.E22 * rhs.E32 + lhs.E32 * rhs.E33;

			res.E03 = lhs.E03 * rhs.E00 + lhs.E13 * rhs.E01 + lhs.E23 * rhs.E02 + lhs.E33 * rhs.E03;
			res.E13 = lhs.E03 * rhs.E10 + lhs.E13 * rhs.E11 + lhs.E23 * rhs.E12 + lhs.E33 * rhs.E13;
			res.E23 = lhs.E03 * rhs.E20 + lhs.E13 * rhs.E21 + lhs.E23 * rhs.E22 + lhs.E33 * rhs.E23;
			res.E33 = lhs.E03 * rhs.E30 + lhs.E13 * rhs.E31 + lhs.E23 * rhs.E32 + lhs.E33 * rhs.E33;

			return res;
		}

		public static bool operator ==(Matrix4x4f lhs, Matrix4x4f rhs)
		{
			return lhs.GetColumn(0) == rhs.GetColumn(0)
				&& lhs.GetColumn(1) == rhs.GetColumn(1)
				&& lhs.GetColumn(2) == rhs.GetColumn(2)
				&& lhs.GetColumn(3) == rhs.GetColumn(3);
		}

		public static bool operator !=(Matrix4x4f lhs, Matrix4x4f rhs)
		{
			return !(lhs == rhs);
		}

		public static Matrix4x4f Scale(Vector3f vector)
		{
			Matrix4x4f m;
			m.E00 = vector.X; m.E10 = 0F; m.E20 = 0F; m.E30 = 0F;
			m.E01 = 0F; m.E11 = vector.Y; m.E21 = 0F; m.E31 = 0F;
			m.E02 = 0F; m.E12 = 0F; m.E22 = vector.Z; m.E32 = 0F;
			m.E03 = 0F; m.E13 = 0F; m.E23 = 0F; m.E33 = 1F;
			return m;
		}

		public static Matrix4x4f Translate(Vector3f vector)
		{
			Matrix4x4f m;
			m.E00 = 1F; m.E10 = 0F; m.E20 = 0F; m.E30 = vector.X;
			m.E01 = 0F; m.E11 = 1F; m.E21 = 0F; m.E31 = vector.Y;
			m.E02 = 0F; m.E12 = 0F; m.E22 = 1F; m.E32 = vector.Z;
			m.E03 = 0F; m.E13 = 0F; m.E23 = 0F; m.E33 = 1F;
			return m;
		}

		public static Matrix4x4f Rotate(Quaternionf q)
		{
			float x = q.X * 2.0F;
			float y = q.Y * 2.0F;
			float z = q.Z * 2.0F;
			float xx = q.X * x;
			float yy = q.Y * y;
			float zz = q.Z * z;
			float xy = q.X * y;
			float xz = q.X * z;
			float yz = q.Y * z;
			float wx = q.W * x;
			float wy = q.W * y;
			float wz = q.W * z;

			Matrix4x4f m;
			m.E00 = 1.0f - (yy + zz); m.E01 = xy + wz; m.E02 = xz - wy; m.E03 = 0.0F;
			m.E10 = xy - wz; m.E11 = 1.0f - (xx + zz); m.E12 = yz + wx; m.E13 = 0.0F;
			m.E20 = xz + wy; m.E21 = yz - wx; m.E22 = 1.0f - (xx + yy); m.E23 = 0.0F;
			m.E30 = 0.0F; m.E31 = 0.0F; m.E32 = 0.0F; m.E33 = 1.0F;
			return m;
		}
	}
}
