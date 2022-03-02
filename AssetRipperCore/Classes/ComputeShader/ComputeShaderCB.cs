using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class  ComputeShaderCB : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Name = reader.ReadAsset<FastPropertyName>();
			ByteSize = reader.ReadInt32();
			Params = reader.ReadAssetArray<ComputeShaderParam>();
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("name", Name.ExportYAML(container));
			node.Add("byteSize", ByteSize);
			node.Add("params", Params.ExportYAML(container));
			return node;
		}

		public FastPropertyName Name { get; set; }
		public int ByteSize { get; set; }
		public ComputeShaderParam[] Params { get; set; }
	}
}
