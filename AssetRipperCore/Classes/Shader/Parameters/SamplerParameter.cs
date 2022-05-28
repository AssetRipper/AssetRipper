using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Shader.Parameters
{
	public sealed class SamplerParameter : IAssetReadable, IYamlExportable
	{
		public SamplerParameter() { }

		public SamplerParameter(uint sampler, int bindPoint)
		{
			Sampler = sampler;
			BindPoint = bindPoint;
		}

		public void Read(AssetReader reader)
		{
			Sampler = reader.ReadUInt32();
			BindPoint = reader.ReadInt32();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("sampler", Sampler);
			node.Add("bindPoint", BindPoint);
			return node;
		}

		public uint Sampler { get; set; }
		public int BindPoint { get; set; }
	}
}
