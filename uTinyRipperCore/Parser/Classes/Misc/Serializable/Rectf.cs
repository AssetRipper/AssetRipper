using System.Globalization;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public struct Rectf : IAsset
	{
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

		public static int ToSerializedVersion(Version version)
		{
			// absolute Min/Max has been replaced by relative values
			if (version.IsGreaterEqual(2))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool HasMinMax(Version version) => version.IsLess(2);

		public void Read(AssetReader reader)
		{
			if (HasMinMax(reader.Version))
			{
				XMin = reader.ReadSingle();
				YMin = reader.ReadSingle();
				XMax = reader.ReadSingle();
				YMax = reader.ReadSingle();
			}
			else
			{
				X = reader.ReadSingle();
				Y = reader.ReadSingle();
				Width = reader.ReadSingle();
				Height = reader.ReadSingle();
			}
		}

		public void Write(AssetWriter writer)
		{
			if (HasMinMax(writer.Version))
			{
				writer.Write(XMin);
				writer.Write(YMin);
				writer.Write(XMax);
				writer.Write(YMax);
			}
			else
			{
				writer.Write(X);
				writer.Write(Y);
				writer.Write(Width);
				writer.Write(Height);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			if (HasMinMax(container.ExportVersion))
			{
				node.Add(XMinName, XMin);
				node.Add(YMinName, YMin);
				node.Add(XMaxName, XMax);
				node.Add(YMaxName, YMax);
			}
			else
			{
				node.Add(XName, X);
				node.Add(YName, Y);
				node.Add(WidthName, Width);
				node.Add(HeightName, Height);
			}
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

		public float XMin
		{
			get => X;
			set => X = value;
		}
		public float YMin
		{
			get => Y;
			set => Y = value;
		}
		public float XMax
		{
			get => X + Width;
			private set => Width = value - XMin;
		}
		public float YMax
		{
			get => Y + Height;
			private set => Height = value - YMin;
		}

		public float X { get; set; }
		public float Y { get; set; }
		public float Width { get; set; }
		public float Height { get; set; }

		public const string XMinName = "xmin";
		public const string YMinName = "ymin";
		public const string XMaxName = "xmax";
		public const string YMaxName = "ymax";
		public const string XName = "x";
		public const string YName = "y";
		public const string WidthName = "width";
		public const string HeightName = "height";
	}
}
