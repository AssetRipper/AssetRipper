namespace UtinyRipper.Classes
{
	public struct ValueConstant : IAssetReadable
	{
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		public static bool IsReadType(Version version)
		{
			return version.IsLess(5, 5);
		}

		public void Read(AssetStream stream)
		{
			ID = stream.ReadUInt32();
			TypeID = stream.ReadUInt32();
			if(IsReadType(stream.Version))
			{
				Type = stream.ReadUInt32();
			}

			Index = stream.ReadUInt32();
		}

		/*public YAMLNode ExportYAML()
		{
#warning TODO: ExportName
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_ID", ID);
			node.Add("m_TypeID", TypeID);
			node.Add("m_Type", Type);
			node.Add("m_Index", Index);
			return node;
		}*/

		public uint ID { get; private set; }
		public uint TypeID { get; private set; }
		public uint Type { get; private set; }
		public uint Index { get; private set; }
	}
}
