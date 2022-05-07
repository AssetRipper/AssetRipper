using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.GraphicsSettings
{
	public sealed class PlatformShaderDefines : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			ShaderPlatform = (GPUPlatform)reader.ReadInt32();
			Defines_Tier1.Read(reader);
			Defines_Tier2.Read(reader);
			Defines_Tier3.Read(reader);
			reader.AlignStream();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(ShaderPlatformName, (int)ShaderPlatform);
			node.Add(Defines_Tier1Name, Defines_Tier1.ExportYaml(container));
			node.Add(Defines_Tier2Name, Defines_Tier2.ExportYaml(container));
			node.Add(Defines_Tier3Name, Defines_Tier3.ExportYaml(container));
			return node;
		}

		public GPUPlatform ShaderPlatform { get; set; }

		public const string ShaderPlatformName = "shaderPlatform";
		public const string Defines_Tier1Name = "defines_Tier1";
		public const string Defines_Tier2Name = "defines_Tier2";
		public const string Defines_Tier3Name = "defines_Tier3";

		public FixedBitset Defines_Tier1 = new();
		public FixedBitset Defines_Tier2 = new();
		public FixedBitset Defines_Tier3 = new();
	}
}
