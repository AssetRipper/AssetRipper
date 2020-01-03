using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Lights
{
	public struct FalloffTable : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Table = new float[13];
			for(int i = 0; i < Table.Length; i++)
			{
				Table[i] = reader.ReadSingle();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(Table0Name, Table[0]);
			node.Add(Table1Name, Table[1]);
			node.Add(Table2Name, Table[2]);
			node.Add(Table3Name, Table[3]);
			node.Add(Table4Name, Table[4]);
			node.Add(Table5Name, Table[5]);
			node.Add(Table6Name, Table[6]);
			node.Add(Table7Name, Table[7]);
			node.Add(Table8Name, Table[8]);
			node.Add(Table9Name, Table[9]);
			node.Add(Table10Name, Table[10]);
			node.Add(Table11Name, Table[11]);
			node.Add(Table12Name, Table[12]);
			return node;
		}

		public float[] Table { get; set; }

		public const string Table0Name = "m_Table[0]";
		public const string Table1Name = "m_Table[1]";
		public const string Table2Name = "m_Table[2]";
		public const string Table3Name = "m_Table[3]";
		public const string Table4Name = "m_Table[4]";
		public const string Table5Name = "m_Table[5]";
		public const string Table6Name = "m_Table[6]";
		public const string Table7Name = "m_Table[7]";
		public const string Table8Name = "m_Table[8]";
		public const string Table9Name = "m_Table[9]";
		public const string Table10Name = "m_Table[10]";
		public const string Table11Name = "m_Table[11]";
		public const string Table12Name = "m_Table[12]";
	}
}
