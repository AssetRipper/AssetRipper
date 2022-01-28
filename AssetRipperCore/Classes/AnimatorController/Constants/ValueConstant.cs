using AssetRipper.Core.Classes.AnimatorController.Editor.AnimatorControllerParameter;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;

namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public sealed class ValueConstant : IAssetReadable
	{
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		public static bool HasType(UnityVersion version) => version.IsLess(5, 5);

		public AnimatorControllerParameterType GetTypeValue(UnityVersion version)
		{
			return HasType(version) ? Type : (AnimatorControllerParameterType)TypeID;
		}

		public void Read(AssetReader reader)
		{
			ID = reader.ReadUInt32();
			TypeID = reader.ReadUInt32();
			if (HasType(reader.Version))
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
		public uint ID { get; set; }
		public uint TypeID { get; set; }
		public AnimatorControllerParameterType Type { get; set; }
		public int Index { get; set; }
	}
}
