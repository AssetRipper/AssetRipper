using System.Collections.Generic;
using System.Globalization;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public struct ColorRGBAf : ISerializableStructure
	{
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
				R = ((color32.RGBA & 0x000000FF) >> 0) / 255.0f,
				G = ((color32.RGBA & 0x0000FF00) >> 8) / 255.0f,
				B = ((color32.RGBA & 0x00FF0000) >> 16) / 255.0f,
				A = ((color32.RGBA & 0xFF000000) >> 24) / 255.0f
			};
			return color;
		}

		public ISerializableStructure CreateDuplicate()
		{
			return new ColorRGBAf();
		}

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

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "[R:{0:0.00} G:{1:0.00} B:{2:0.00} A:{3:0.00}]", R, G, B, A);
		}

		public static ColorRGBAf White => new ColorRGBAf(1.0f, 1.0f, 1.0f, 1.0f);

		public const string RName = "r";
		public const string GName = "g";
		public const string BName = "b";
		public const string AName = "a";

		public float R { get; private set; }
		public float G { get; private set; }
		public float B { get; private set; }
		public float A { get; private set; }
	}
}
