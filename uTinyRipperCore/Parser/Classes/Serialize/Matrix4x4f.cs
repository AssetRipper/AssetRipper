using System.Collections.Generic;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public struct Matrix4x4f : ISerializableStructure
	{
		public ISerializableStructure CreateDuplicate()
		{
			return new Matrix4x4f();
		}

		public void Read(AssetReader reader)
		{
			E00 = reader.ReadSingle();
			E01 = reader.ReadSingle();
			E02 = reader.ReadSingle();
			E03 = reader.ReadSingle();
			E10 = reader.ReadSingle();
			E11 = reader.ReadSingle();
			E12 = reader.ReadSingle();
			E13 = reader.ReadSingle();
			E20 = reader.ReadSingle();
			E21 = reader.ReadSingle();
			E22 = reader.ReadSingle();
			E23 = reader.ReadSingle();
			E30 = reader.ReadSingle();
			E31 = reader.ReadSingle();
			E32 = reader.ReadSingle();
			E33 = reader.ReadSingle();
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

		public static Matrix4x4f Identity => new Matrix4x4f { E00 = 1.0f, E11 = 1.0f, E22 = 1.0f, E33 = 1.0f };

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
