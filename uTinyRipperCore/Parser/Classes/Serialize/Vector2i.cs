using System.Collections.Generic;
using System.IO;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public struct Vector2i : ISerializableStructure
	{
		public Vector2i(int x, int y)
		{
			X = x;
			Y = y;
		}

		public static bool operator ==(Vector2i left, Vector2i right)
		{
			return left.X == right.X && left.Y == right.Y;
		}

		public static bool operator !=(Vector2i left, Vector2i right)
		{
			return left.X != right.X || left.Y != right.Y;
		}

		public ISerializableStructure CreateDuplicate()
		{
			return new Vector2i();
		}

		public void Read(AssetReader reader)
		{
			X = reader.ReadInt32();
			Y = reader.ReadInt32();
		}

		public void Write(BinaryWriter stream)
		{
			stream.Write(X);
			stream.Write(Y);
		}

		public YAMLNode ExportYAML(IExportContainer container)
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

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType() != typeof(Vector2i))
			{
				return false;
			}
			return this == (Vector2i)obj;
		}

		public override int GetHashCode()
		{
			int hash = 941;
			unchecked
			{
				hash = hash + 61 * X.GetHashCode();
				hash = hash * 677 + Y.GetHashCode();
			}
			return hash;
		}

		public override string ToString()
		{
			return $"[{X}, {Y}]";
		}

		public int X { get; private set; }
		public int Y { get; private set; }

		public const string XName = "x";
		public const string YName = "y";
	}
}
