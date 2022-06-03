using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedProperty : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			NameString = reader.ReadString();
			Description = reader.ReadString();
			Attributes = reader.ReadAssetArray<Utf8StringLegacy>();
			Type = (SerializedPropertyType)reader.ReadInt32();
			Flags = (SerializedPropertyFlag)reader.ReadUInt32();
			DefValue0 = reader.ReadSingle();
			DefValue1 = reader.ReadSingle();
			DefValue2 = reader.ReadSingle();
			DefValue3 = reader.ReadSingle();
			m_DefTexture.Read(reader);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("m_Name", NameString);
			node.Add("m_Description", Description);
			node.Add("m_Attributes", Attributes.ExportYaml(container));
			node.Add("m_Type", (int)Type);
			node.Add("m_Flags", (uint)Flags);
			node.Add("m_DefValue[0]", DefValue0);
			node.Add("m_DefValue[1]", DefValue1);
			node.Add("m_DefValue[2]", DefValue2);
			node.Add("m_DefValue[3]", DefValue3);
			node.Add("m_DefTexture", m_DefTexture.ExportYaml(container));
			return node;
		}

		public string NameString { get; set; }
		public string Description { get; set; }
		public Utf8StringBase[] Attributes { get; set; }
		public SerializedPropertyType Type { get; set; }
		public SerializedPropertyFlag Flags { get; set; }
		public float DefValue0 { get; set; }
		public float DefValue1 { get; set; }
		public float DefValue2 { get; set; }
		public float DefValue3 { get; set; }

		public SerializedTextureProperty DefTexture => m_DefTexture;

		private SerializedTextureProperty m_DefTexture = new();
	}
}
