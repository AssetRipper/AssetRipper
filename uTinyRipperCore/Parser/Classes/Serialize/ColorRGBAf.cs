using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public struct ColorRGBAf : IScriptStructure
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

		public IScriptStructure CreateCopy()
		{
			return this;
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
			node.Add("r", R);
			node.Add("g", G);
			node.Add("b", B);
			node.Add("a", A);
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		public static ColorRGBAf White => new ColorRGBAf(1.0f, 1.0f, 1.0f, 1.0f);

		public IScriptStructure Base => null;
		public string Namespace => ScriptType.UnityEngineName;
		public string Name => ScriptType.ColorName;

		public float R { get; private set; }
		public float G { get; private set; }
		public float B { get; private set; }
		public float A { get; private set; }
	}
}
