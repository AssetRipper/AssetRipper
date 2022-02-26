using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class  ComputeShaderResource : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			GeneratedName = reader.ReadString();
			BindPoint = reader.ReadInt32();
			SamplerBindPoint = reader.ReadInt32();
			TexDimension = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("name", Name);
			node.Add("generatedName", GeneratedName);
			node.Add("bindPoint", BindPoint);
			node.Add("samplerBindPoint", SamplerBindPoint);
			node.Add("texDimension", TexDimension);
			return node;
		}

		public string Name { get; set; }
		public string GeneratedName { get; set; }
		public int BindPoint { get; set; }
		public int SamplerBindPoint { get; set; }
		public int TexDimension { get; set; }
	}
}
