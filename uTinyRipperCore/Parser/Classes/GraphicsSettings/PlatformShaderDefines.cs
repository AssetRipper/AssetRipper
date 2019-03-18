using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Shaders;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.GraphicsSettingss
{
	public struct PlatformShaderDefines : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			ShaderPlatform = (GPUPlatform)reader.ReadInt32();
			Defines_Tier1.Read(reader);
			Defines_Tier2.Read(reader);
			Defines_Tier3.Read(reader);
			reader.AlignStream(AlignType.Align4);			
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("shaderPlatform", (int)ShaderPlatform);
			node.Add("defines_Tier1", Defines_Tier1.ExportYAML(container));
			node.Add("defines_Tier2", Defines_Tier2.ExportYAML(container));
			node.Add("defines_Tier3", Defines_Tier3.ExportYAML(container));
			return node;
		}

		public GPUPlatform ShaderPlatform { get; private set; }

		public FixedBitset Defines_Tier1;
		public FixedBitset Defines_Tier2;
		public FixedBitset Defines_Tier3;
	}
}
