using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;

using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class  ComputeShaderCB : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Name = reader.ReadAsset<FastPropertyName>();
			ByteSize = reader.ReadInt32();
			Params = reader.ReadAssetArray<ComputeShaderParam>();
			reader.AlignStream();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("name", Name.ExportYaml(container));
			node.Add("byteSize", ByteSize);
			node.Add("params", Params.ExportYaml(container));
			return node;
		}

		public FastPropertyName Name { get; set; }
		public int ByteSize { get; set; }
		public ComputeShaderParam[] Params { get; set; }
	}
}
