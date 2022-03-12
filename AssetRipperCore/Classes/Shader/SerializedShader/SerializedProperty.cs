using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedProperty : IAssetReadable, ISerializedProperty, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
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

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Name", Name);
			node.Add("m_Description", Description);
			node.Add("m_Attributes", Attributes.ExportYAML(container));
			node.Add("m_Type", (int)Type);
			node.Add("m_Flags", (uint)Flags);
			node.Add("m_DefValue[0]", DefValue0);
			node.Add("m_DefValue[1]", DefValue1);
			node.Add("m_DefValue[2]", DefValue2);
			node.Add("m_DefValue[3]", DefValue3);
			node.Add("m_DefTexture", m_DefTexture.ExportYAML(container));
			return node;
		}

		public string Name { get; set; }
		public string Description { get; set; }
		public Utf8StringBase[] Attributes { get; set; }
		public SerializedPropertyType Type { get; set; }
		public SerializedPropertyFlag Flags { get; set; }
		public float DefValue0 { get; set; }
		public float DefValue1 { get; set; }
		public float DefValue2 { get; set; }
		public float DefValue3 { get; set; }

		public ISerializedTextureProperty DefTexture => m_DefTexture;

		private SerializedTextureProperty m_DefTexture = new();
	}
}
