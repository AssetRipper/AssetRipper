using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace AssetRipper.Core.Math
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct ColorRGBAf : IAsset, IEquatable<ColorRGBAf>
	{
		public float R;
		public float G;
		public float B;
		public float A;

		public const string RName = "r";
		public const string GName = "g";
		public const string BName = "b";
		public const string AName = "a";

		public ColorRGBAf(float r, float g, float b, float a)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public static explicit operator ColorRGBAf(ColorRGBA32 color32)
		{
			ColorRGBAf color = new ColorRGBAf
			{
				R = color32.R / 255.0f,
				G = color32.G / 255.0f,
				B = color32.B / 255.0f,
				A = color32.A / 255.0f
			};
			return color;
		}

		public static explicit operator Vector4f(ColorRGBAf c) => new Vector4f(c.R, c.G, c.B, c.A);

		public void Read(AssetReader reader)
		{
			R = reader.ReadSingle();
			G = reader.ReadSingle();
			B = reader.ReadSingle();
			A = reader.ReadSingle();
		}

		public void Read32(AssetReader reader)
		{
			ColorRGBA32 color32 = new ColorRGBA32();
			color32.Read(reader);
			this = (ColorRGBAf)color32;
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(R);
			writer.Write(G);
			writer.Write(B);
			writer.Write(A);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Style = MappingStyle.Flow;
			node.Add(RName, R);
			node.Add(GName, G);
			node.Add(BName, B);
			node.Add(AName, A);
			return node;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "[R:{0:0.00} G:{1:0.00} B:{2:0.00} A:{3:0.00}]", R, G, B, A);
		}

		public override int GetHashCode()
		{
			return ((Vector4f)this).GetHashCode();
		}

		public override bool Equals(object other)
		{
			if (!(other is ColorRGBAf))
				return false;
			return Equals((ColorRGBAf)other);
		}

		public bool Equals(ColorRGBAf other)
		{
			return R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A);
		}

		public static ColorRGBAf operator +(ColorRGBAf a, ColorRGBAf b)
		{
			return new ColorRGBAf(a.R + b.R, a.G + b.G, a.B + b.B, a.A + b.A);
		}

		public static ColorRGBAf operator -(ColorRGBAf a, ColorRGBAf b)
		{
			return new ColorRGBAf(a.R - b.R, a.G - b.G, a.B - b.B, a.A - b.A);
		}

		public static ColorRGBAf operator *(ColorRGBAf a, ColorRGBAf b)
		{
			return new ColorRGBAf(a.R * b.R, a.G * b.G, a.B * b.B, a.A * b.A);
		}

		public static ColorRGBAf operator *(ColorRGBAf a, float b)
		{
			return new ColorRGBAf(a.R * b, a.G * b, a.B * b, a.A * b);
		}

		public static ColorRGBAf operator *(float b, ColorRGBAf a)
		{
			return new ColorRGBAf(a.R * b, a.G * b, a.B * b, a.A * b);
		}

		public static ColorRGBAf operator /(ColorRGBAf a, float b)
		{
			return new ColorRGBAf(a.R / b, a.G / b, a.B / b, a.A / b);
		}

		public static bool operator ==(ColorRGBAf lhs, ColorRGBAf rhs)
		{
			return (Vector4f)lhs == (Vector4f)rhs;
		}

		public static bool operator !=(ColorRGBAf lhs, ColorRGBAf rhs)
		{
			return !(lhs == rhs);
		}

		public static ColorRGBAf Black => new ColorRGBAf(0.0f, 0.0f, 0.0f, 1.0f);
		public static ColorRGBAf White => new ColorRGBAf(1.0f, 1.0f, 1.0f, 1.0f);
	}
}
