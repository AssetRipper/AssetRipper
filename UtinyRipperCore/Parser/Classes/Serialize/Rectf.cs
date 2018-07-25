using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public struct Rectf : IAssetReadable, IYAMLExportable
	{
		private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}

		public Rectf(float x, float y, float width, float height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		public static bool operator == (Rectf left, Rectf right)
		{
			if(left.X != right.X)
			{
				return false;
			}
			if (left.Y != right.Y)
			{
				return false;
			}
			if(left.Height != right.Height)
			{
				return false;
			}
			if(left.Width != right.Width)
			{
				return false;
			}
			return true;
		}

		public static bool operator !=(Rectf left, Rectf right)
		{
			if (left.X != right.X)
			{
				return true;
			}
			if (left.Y != right.Y)
			{
				return true;
			}
			if (left.Height != right.Height)
			{
				return true;
			}
			if (left.Width != right.Width)
			{
				return true;
			}
			return false;
		}

		public void Read(AssetStream stream)
		{
			X = stream.ReadSingle();
			Y = stream.ReadSingle();
			Width = stream.ReadSingle();
			Height = stream.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("x", X);
			node.Add("y", Y);
			node.Add("width", Width);
			node.Add("height", Height);
			return node;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType() != typeof(Rectf))
			{
				return false;
			}
			return this == (Rectf)obj;
		}

		public override int GetHashCode()
		{
			int hash = 97;
			unchecked
			{
				hash = hash * 53 + X.GetHashCode();
				hash = hash * 53 + Y.GetHashCode();
				hash = hash * 53 + Width.GetHashCode();
				hash = hash * 53 + Height.GetHashCode();
			}
			return hash;
		}

		public override string ToString()
		{
			return $"[X:{X:0.00}, Y:{Y:0.00}, W:{Width:0.00}, H:{Height:0.00}]";
		}

		public bool ContainsCorner(Vector2f position)
		{
			if(X != position.X && X + Width != position.X)
			{
				return false;
			}
			if (Y != position.Y && Y + Height != position.Y)
			{
				return false;
			}
			return true;
		}

		public Vector2f Center => new Vector2f(X + Width / 2.0f, Y + Height / 2.0f);

		public float X { get; private set; }
		public float Y { get; private set; }
		public float Width { get; private set; }
		public float Height { get; private set; }
	}
}
