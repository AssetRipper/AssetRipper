using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class  ComputeShaderParam : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Name = reader.ReadAsset<FastPropertyName>();
			Type = reader.ReadInt32();
			Offset = reader.ReadInt32();
			ArraySize = reader.ReadInt32();
			RowCount = reader.ReadInt32();
			ColCount = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("name", Name.ExportYAML(container));
			node.Add("type", Type);
			node.Add("offset", Offset);
			node.Add("arraySize", ArraySize);
			node.Add("rowCount", RowCount);
			node.Add("colCount", ColCount);
			return node;
		}

		public FastPropertyName Name { get; set; }
		public int Type { get; set; }
		public int Offset { get; set; }
		public int ArraySize { get; set; }
		public int RowCount { get; set; }
		public int ColCount { get; set; }
	}
}
