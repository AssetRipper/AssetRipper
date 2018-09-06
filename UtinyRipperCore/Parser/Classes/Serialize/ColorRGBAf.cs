using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public struct ColorRGBAf : IScriptStructure
	{
		public ColorRGBAf(ColorRGBA32 color):
			this(color.RGBA)
		{
		}

		public ColorRGBAf(ColorRGBAf copy)
		{
			R = copy.R;
			G = copy.G;
			B = copy.B;
			A = copy.A;
		}

		public ColorRGBAf(uint value32)
		{
			R = (value32 & 0xFF000000 >> 24) / 255.0f;
			G = (value32 & 0x00FF0000 >> 16) / 255.0f;
			B = (value32 & 0x0000FF00 >> 8) / 255.0f;
			A = (value32 & 0x000000FF >> 0) / 255.0f;
		}

		public ColorRGBAf(float r, float g, float b, float a)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public IScriptStructure CreateCopy()
		{
			return new ColorRGBAf(this);
		}

		public void Read(AssetReader reader)
		{
			R = reader.ReadSingle();
			G = reader.ReadSingle();
			B = reader.ReadSingle();
			A = reader.ReadSingle();
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

		public IScriptStructure Base => null;
		public string Namespace => ScriptType.UnityEngineName;
		public string Name => ScriptType.ColorName;

		public float R { get; private set; }
		public float G { get; private set; }
		public float B { get; private set; }
		public float A { get; private set; }
	}
}
