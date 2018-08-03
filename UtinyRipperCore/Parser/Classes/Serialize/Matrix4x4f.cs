using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public struct Matrix4x4f : IScriptStructure
	{
		public Matrix4x4f(Matrix4x4f copy)
		{
			E00 = copy.E00;
			E01 = copy.E01;
			E02 = copy.E02;
			E03 = copy.E03;
			E10 = copy.E10;
			E11 = copy.E11;
			E12 = copy.E12;
			E13 = copy.E13;
			E20 = copy.E20;
			E21 = copy.E21;
			E22 = copy.E22;
			E23 = copy.E23;
			E30 = copy.E30;
			E31 = copy.E31;
			E32 = copy.E32;
			E33 = copy.E33;
		}

		public IScriptStructure CreateCopy()
		{
			return new Matrix4x4f(this);
		}

		public void Read(AssetStream stream)
		{
			E00 = stream.ReadSingle();
			E01 = stream.ReadSingle();
			E02 = stream.ReadSingle();
			E03 = stream.ReadSingle();
			E10 = stream.ReadSingle();
			E11 = stream.ReadSingle();
			E12 = stream.ReadSingle();
			E13 = stream.ReadSingle();
			E20 = stream.ReadSingle();
			E21 = stream.ReadSingle();
			E22 = stream.ReadSingle();
			E23 = stream.ReadSingle();
			E30 = stream.ReadSingle();
			E31 = stream.ReadSingle();
			E32 = stream.ReadSingle();
			E33 = stream.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("e00", E00);
			node.Add("e01", E01);
			node.Add("e02", E02);
			node.Add("e03", E03);
			node.Add("e10", E10);
			node.Add("e11", E11);
			node.Add("e12", E12);
			node.Add("e13", E13);
			node.Add("e20", E20);
			node.Add("e21", E21);
			node.Add("e22", E22);
			node.Add("e23", E23);
			node.Add("e30", E30);
			node.Add("e31", E31);
			node.Add("e32", E32);
			node.Add("e33", E33);
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		public float E00 { get; private set; }
		public float E01 { get; private set; }
		public float E02 { get; private set; }
		public float E03 { get; private set; }
		public float E10 { get; private set; }
		public float E11 { get; private set; }
		public float E12 { get; private set; }
		public float E13 { get; private set; }
		public float E20 { get; private set; }
		public float E21 { get; private set; }
		public float E22 { get; private set; }
		public float E23 { get; private set; }
		public float E30 { get; private set; }
		public float E31 { get; private set; }
		public float E32 { get; private set; }
		public float E33 { get; private set; }
	}
}
