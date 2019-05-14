using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public struct Vector3i : ISerializableStructure
	{
		public Vector3i(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static bool operator ==(Vector3i left, Vector3i right)
		{
			return left.X == right.X && left.Y == right.Y && left.Z == right.Z;
		}

		public static bool operator !=(Vector3i left, Vector3i right)
		{
			return left.X != right.X || left.Y != right.Y || left.Z != right.Z;
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

		public ISerializableStructure CreateDuplicate()
		{
			return new Vector3i();
		}

		public void Read(AssetReader reader)
		{
			X = reader.ReadInt32();
			Y = reader.ReadInt32();
			Z = reader.ReadInt32();
		}

		public void Write(BinaryWriter stream)
		{
			stream.Write(X);
			stream.Write(Y);
			stream.Write(Z);
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

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		public override bool Equals(object obj)
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
				hash = hash + 787 * X.GetHashCode();
				hash = hash * 823 + Y.GetHashCode();
				hash = hash * 431 + Z.GetHashCode();
			}
			return hash;
		}

		public override string ToString()
		{
			return $"[{X}, {Y}, {Z}]";
		}

		public int X { get; private set; }
		public int Y { get; private set; }
		public int Z { get; private set; }

		public const string XName = "x";
		public const string YName = "y";
		public const string ZName = "z";
	}
}
