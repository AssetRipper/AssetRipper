using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Lights
{
	public struct FalloffTable : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_table = new float[13];
			for(int i = 0; i < m_table.Length; i++)
			{
				m_table[i] = reader.ReadSingle();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Table[0]", Table[0]);
			node.Add("m_Table[1]", Table[1]);
			node.Add("m_Table[2]", Table[2]);
			node.Add("m_Table[3]", Table[3]);
			node.Add("m_Table[4]", Table[4]);
			node.Add("m_Table[5]", Table[5]);
			node.Add("m_Table[6]", Table[6]);
			node.Add("m_Table[7]", Table[7]);
			node.Add("m_Table[8]", Table[8]);
			node.Add("m_Table[9]", Table[9]);
			node.Add("m_Table[10]", Table[10]);
			node.Add("m_Table[11]", Table[11]);
			node.Add("m_Table[12]", Table[12]);
			return node;
		}

		public IReadOnlyList<float> Table => m_table;

		private float[] m_table;
	}
}
