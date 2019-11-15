using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Layout;

namespace uTinyRipper.Classes
{
	public struct Vector2i : IAsset
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

		public void Read(AssetReader reader)
		{
			X = reader.ReadInt32();
			Y = reader.ReadInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			Vector2iLayout layout = container.ExportLayout.Serialized.Vector2i;
			node.Style = MappingStyle.Flow;
			node.Add(layout.XName, X);
			node.Add(layout.YName, Y);
			return node;
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

		public int X { get; set; }
		public int Y { get; set; }
	}
}
