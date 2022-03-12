using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Shader.Parameters
{
	public sealed class UAVParameter : IAssetReadable, IYAMLExportable
	{
		public UAVParameter() { }

		public UAVParameter(string name, int index, int originalIndex)
		{
			Name = name;
			NameIndex = -1;
			Index = index;
			OriginalIndex = originalIndex;
		}

		public void Read(AssetReader reader)
		{
			NameIndex = reader.ReadInt32();
			Index = reader.ReadInt32();
			OriginalIndex = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_NameIndex", NameIndex);
			node.Add("m_Index", Index);
			node.Add("m_OriginalIndex", OriginalIndex);
			return node;
		}

		public string Name { get; set; }
		public int NameIndex { get; set; }
		public int Index { get; set; }
		public int OriginalIndex { get; set; }
	}
}
