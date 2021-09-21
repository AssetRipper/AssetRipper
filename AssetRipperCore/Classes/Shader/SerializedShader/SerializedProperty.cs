using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public struct SerializedProperty : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			Description = reader.ReadString();
			Attributes = reader.ReadStringArray();
			Type = (SerializedPropertyType)reader.ReadInt32();
			Flags = (SerializedPropertyFlag)reader.ReadUInt32();
			DefValue0 = reader.ReadSingle();
			DefValue1 = reader.ReadSingle();
			DefValue2 = reader.ReadSingle();
			DefValue3 = reader.ReadSingle();
			DefTexture.Read(reader);
		}

		public string Name { get; set; }
		public string Description { get; set; }
		public string[] Attributes { get; set; }
		public SerializedPropertyType Type { get; set; }
		public SerializedPropertyFlag Flags { get; set; }
		public float DefValue0 { get; set; }
		public float DefValue1 { get; set; }
		public float DefValue2 { get; set; }
		public float DefValue3 { get; set; }

		public SerializedTextureProperty DefTexture;
	}
}
