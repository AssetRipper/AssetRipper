using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Math.Vectors
{
	public sealed class Vector3i : IAsset, IVector3i
	{
		public Vector3i() { }
		public Vector3i(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static bool operator ==(Vector3i left, Vector3i right)
		{
			if (left is null)
			{
				return right is null;
			}
			else if (right is null)
			{
				return false;
			}
			else
			{
				return left.X == right.X && left.Y == right.Y && left.Z == right.Z;
			}
		}

		public static bool operator !=(Vector3i left, Vector3i right)
		{
			return !(left == right);
		}

		public int GetValueByMember(int member)
		{
			member %= 3;
			if (member == 0)
			{
				return X;
			}

			if (member == 1)
			{
				return Y;
			}

			return Z;
		}

		public int GetMemberByValue(int value)
		{
			if (X == value)
			{
				return 0;
			}

			if (Y == value)
			{
				return 1;
			}

			if (Z == value)
			{
				return 2;
			}

			throw new ArgumentException($"Member with value {value} wasn't found");
		}

		public bool ContainsValue(int value)
		{
			if (X == value || Y == value || Z == value)
			{
				return true;
			}

			return false;
		}

		public void Read(AssetReader reader)
		{
			X = reader.ReadInt32();
			Y = reader.ReadInt32();
			Z = reader.ReadInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Z);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Style = MappingStyle.Flow;
			node.Add(XName, X);
			node.Add(YName, Y);
			node.Add(ZName, Z);
			return node;
		}

		public override bool Equals(object? obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (obj.GetType() != typeof(Vector3i))
			{
				return false;
			}

			return this == (Vector3i)obj;
		}

		public override int GetHashCode()
		{
			int hash = 193;
			unchecked
			{
				hash = hash + (787 * X.GetHashCode());
				hash = (hash * 823) + Y.GetHashCode();
				hash = (hash * 431) + Z.GetHashCode();
			}
			return hash;
		}

		public override string ToString()
		{
			return $"[{X}, {Y}, {Z}]";
		}

		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }

		public const string XName = "x";
		public const string YName = "y";
		public const string ZName = "z";
	}
}
