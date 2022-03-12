using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Shader.Parameters
{
	public sealed class VectorParameter : IAssetReadable, IYAMLExportable
	{
		public VectorParameter() { }

		public VectorParameter(string name, ShaderParamType type, int index, int columns)
		{
			Name = name;
			NameIndex = -1;
			Index = index;
			ArraySize = 0;
			Type = type;
			Dim = (byte)columns;
		}

		public VectorParameter(string name, ShaderParamType type, int index, int arraySize, int columns) : this(name, type, index, columns)
		{
			ArraySize = arraySize;
		}

		public void Read(AssetReader reader)
		{
			NameIndex = reader.ReadInt32();
			Index = reader.ReadInt32();
			ArraySize = reader.ReadInt32();
			Type = (ShaderParamType)reader.ReadByte();
			Dim = reader.ReadByte();
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_NameIndex", NameIndex);
			node.Add("m_Index", Index);
			node.Add("m_ArraySize", ArraySize);
			node.Add("m_Type", (byte)Type);
			node.Add("m_Dim", Dim);
			return node;
		}

		public string Name { get; set; }
		public int NameIndex { get; set; }
		public int Index { get; set; }
		public int ArraySize { get; set; }
		public ShaderParamType Type { get; set; }
		public byte Dim { get; set; }
	}
}
