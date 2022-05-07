using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Shader.Parameters
{
	public sealed class StructParameter : IAssetReadable, IYamlExportable
	{
		public StructParameter() { }

		public StructParameter(string name, int index, int arraySize, int structSize, VectorParameter[] vectors, MatrixParameter[] matrices)
		{
			Name = name;
			NameIndex = -1;
			Index = index;
			ArraySize = arraySize;
			StructSize = structSize;
			VectorMembers = vectors;
			MatrixMembers = matrices;
		}

		public void Read(AssetReader reader)
		{
			NameIndex = reader.ReadInt32();
			Index = reader.ReadInt32();
			ArraySize = reader.ReadInt32();
			StructSize = reader.ReadInt32();
			VectorMembers = reader.ReadAssetArray<VectorParameter>();
			reader.AlignStream();
			MatrixMembers = reader.ReadAssetArray<MatrixParameter>();
			reader.AlignStream();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("m_NameIndex", NameIndex);
			node.Add("m_Index", Index);
			node.Add("m_ArraySize", ArraySize);
			node.Add("m_StructSize", StructSize);
			node.Add("m_VectorMembers", VectorMembers.ExportYaml(container));
			node.Add("m_MatrixMembers", MatrixMembers.ExportYaml(container));
			return node;
		}

		public string Name { get; set; }
		public int NameIndex { get; set; }
		public int Index { get; set; }
		public int ArraySize { get; set; }
		public int StructSize { get; set; }
		public VectorParameter[] VectorMembers { get; set; }
		public MatrixParameter[] MatrixMembers { get; set; }
	}
}
