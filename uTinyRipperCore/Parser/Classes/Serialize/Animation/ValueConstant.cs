using uTinyRipper.Classes.AnimatorControllers;

namespace uTinyRipper.Classes
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

		public AnimatorControllerParameterType GetTypeValue(Version version)
		{
			return IsReadType(version) ? Type : (AnimatorControllerParameterType)TypeID;
		}

		public void Read(AssetReader reader)
		{
			ID = reader.ReadUInt32();
			TypeID = reader.ReadUInt32();
			if(IsReadType(reader.Version))
			{
				Type = (AnimatorControllerParameterType)reader.ReadUInt32();
			}

			Index = (int)reader.ReadUInt32();
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

		/// <summary>
		/// Unique ID. Key in dictionary
		/// </summary>
		public uint ID { get; private set; }
		public uint TypeID { get; private set; }
		public AnimatorControllerParameterType Type { get; private set; }
		public int Index { get; private set; }
	}
}
