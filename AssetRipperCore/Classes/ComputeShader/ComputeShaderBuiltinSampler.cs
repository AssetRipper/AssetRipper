using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class  ComputeShaderBuiltinSampler : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Sampler = reader.ReadUInt32();
			BindPoint = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("sampler", Sampler);
			node.Add("bindPoint", BindPoint);
			return node;
		}

		public uint Sampler { get; set; }
		public int BindPoint { get; set; }
	}
}
