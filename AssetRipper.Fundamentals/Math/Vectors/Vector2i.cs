using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Math.Vectors
{
	public sealed class Vector2i : IAsset, IVector2i
	{
		public Vector2i() { }
		public Vector2i(int x, int y)
		{
			X = x;
			Y = y;
		}

		public static bool operator ==(Vector2i left, Vector2i right)
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
				return left.X == right.X && left.Y == right.Y;
			}
		}

		public static bool operator !=(Vector2i left, Vector2i right)
		{
			return !(left == right);
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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Style = MappingStyle.Flow;
			node.Add(XName, X);
			node.Add(YName, Y);
			return node;
		}

		public override bool Equals(object? obj)
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
				hash = hash + (61 * X.GetHashCode());
				hash = (hash * 677) + Y.GetHashCode();
			}
			return hash;
		}

		public override string ToString()
		{
			return $"[{X}, {Y}]";
		}

		public int X { get; set; }
		public int Y { get; set; }

		public const string XName = "x";
		public const string YName = "y";
	}
}
