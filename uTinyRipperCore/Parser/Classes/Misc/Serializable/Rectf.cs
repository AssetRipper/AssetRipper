using System.Collections.Generic;
using System.Globalization;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public struct Rectf : ISerializableStructure
	{
		private static int GetSerializedVersion(Version version)
		{
			// TODO:
			return 2;
		}
		
		public Rectf(float x, float y, float width, float height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		public Rectf(Vector2f positon, Vector2f size):
			this(positon.X, positon.Y, size.X, size.Y)
		{
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

		public static Rectf operator +(Rectf left, Rectf right)
		{
			Rectf result = default;
			result.X = left.X + right.X;
			result.Y = left.Y + right.Y;
			result.Width = left.Width + right.Width;
			result.Height = left.Height + right.Height;
			return result;
		}

		public static Rectf operator -(Rectf left, Rectf right)
		{
			Rectf result = default;
			result.X = left.X - right.X;
			result.Y = left.Y - right.Y;
			result.Width = left.Width - right.Width;
			result.Height = left.Height - right.Height;
			return result;
		}

		public ISerializableStructure CreateDuplicate()
		{
			return new Rectf();
		}

		public void Read(AssetReader reader)
		{
			X = reader.ReadSingle();
			Y = reader.ReadSingle();
			Width = reader.ReadSingle();
			Height = reader.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add("x", X);
			node.Add("y", Y);
			node.Add("width", Width);
			node.Add("height", Height);
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
				hash = hash + 347 * X.GetHashCode();
				hash = hash * 53 + Y.GetHashCode();
				hash = hash * 641 + Width.GetHashCode();
				hash = hash * 557 + Height.GetHashCode();
			}
			return hash;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "[X:{0:0.00}, Y:{1:0.00}, W:{2:0.00}, H:{3:0.00}", X, Y, Width, Height);
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

		public Vector2f Position => new Vector2f(X, Y);
		public Vector2f Size => new Vector2f(Width, Height);

		public float X { get; private set; }
		public float Y { get; private set; }
		public float Width { get; private set; }
		public float Height { get; private set; }
	}
}
