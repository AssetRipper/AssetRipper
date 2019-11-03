using uTinyRipper.Classes.Misc;
using uTinyRipper.Classes.Shaders;
using uTinyRipper.Converters;
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
			reader.AlignStream();			
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(ShaderPlatformName, (int)ShaderPlatform);
			node.Add(Defines_Tier1Name, Defines_Tier1.ExportYAML(container));
			node.Add(Defines_Tier2Name, Defines_Tier2.ExportYAML(container));
			node.Add(Defines_Tier3Name, Defines_Tier3.ExportYAML(container));
			return node;
		}

		public GPUPlatform ShaderPlatform { get; set; }

		public const string ShaderPlatformName = "shaderPlatform";
		public const string Defines_Tier1Name = "defines_Tier1";
		public const string Defines_Tier2Name = "defines_Tier2";
		public const string Defines_Tier3Name = "defines_Tier3";

		public FixedBitset Defines_Tier1;
		public FixedBitset Defines_Tier2;
		public FixedBitset Defines_Tier3;
	}
}
