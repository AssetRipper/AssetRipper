using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Globalization;

namespace AssetRipper.Core.Math
{
	public sealed class Rectf : IAsset, IRectf
	{
		public float X { get; set; }
		public float Y { get; set; }
		public float Width { get; set; }
		public float Height { get; set; }

		public const string XName = "x";
		public const string YName = "y";
		public const string WidthName = "width";
		public const string HeightName = "height";

		public Rectf() { }

		public Rectf(float x, float y, float width, float height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		public Rectf(Vector2f positon, Vector2f size) : this(positon.X, positon.Y, size.X, size.Y) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			return 2; //2.0.0 and higher
		}

		public static bool operator ==(Rectf left, Rectf right)
		{
			if (left.X != right.X)
			{
				return false;
			}
			if (left.Y != right.Y)
			{
				return false;
			}
			if (left.Height != right.Height)
			{
				return false;
			}
			if (left.Width != right.Width)
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
			Rectf result = new();
			result.X = left.X + right.X;
			result.Y = left.Y + right.Y;
			result.Width = left.Width + right.Width;
			result.Height = left.Height + right.Height;
			return result;
		}

		public static Rectf operator -(Rectf left, Rectf right)
		{
			Rectf result = new();
			result.X = left.X - right.X;
			result.Y = left.Y - right.Y;
			result.Width = left.Width - right.Width;
			result.Height = left.Height - right.Height;
			return result;
		}

		public void Read(AssetReader reader)
		{
			X = reader.ReadSingle();
			Y = reader.ReadSingle();
			Width = reader.ReadSingle();
			Height = reader.ReadSingle();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Width);
			writer.Write(Height);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(XName, X);
			node.Add(YName, Y);
			node.Add(WidthName, Width);
			node.Add(HeightName, Height);
			return node;
		}

		public override bool Equals(object? obj)
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
				hash = hash + (347 * X.GetHashCode());
				hash = (hash * 53) + Y.GetHashCode();
				hash = (hash * 641) + Width.GetHashCode();
				hash = (hash * 557) + Height.GetHashCode();
			}
			return hash;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "[X:{0:0.00}, Y:{1:0.00}, W:{2:0.00}, H:{3:0.00}", X, Y, Width, Height);
		}

		public bool ContainsCorner(Vector2f position)
		{
			if (X != position.X && X + Width != position.X)
			{
				return false;
			}
			if (Y != position.Y && Y + Height != position.Y)
			{
				return false;
			}
			return true;
		}

		public Vector2f Center => new Vector2f(X + (Width / 2.0f), Y + (Height / 2.0f));

		public Vector2f Position => new Vector2f(X, Y);
		public Vector2f Size => new Vector2f(Width, Height);
		public Vector2f Min
		{
			get => new Vector2f(XMin, YMin);
			set
			{
				XMin = value.X;
				YMin = value.Y;
			}
		}
		public Vector2f Max
		{
			get => new Vector2f(XMax, YMax);
			set
			{
				XMax = value.X;
				YMax = value.Y;
			}
		}

		public float XMin
		{
			get => X;
			set
			{
				float delta = X - value;
				X = value;
				Width += delta;
			}
		}
		public float YMin
		{
			get => Y;
			set
			{
				float delta = Y - value;
				Y = value;
				Height += delta;
			}
		}
		public float XMax
		{
			get => X + Width;
			set => Width = value - XMin;
		}
		public float YMax
		{
			get => Y + Height;
			set => Height = value - YMin;
		}
	}
}
