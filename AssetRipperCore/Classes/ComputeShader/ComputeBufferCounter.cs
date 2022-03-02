using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class ComputeBufferCounter : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Bindpoint = reader.ReadInt32();
			Offset = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("bindpoint", Bindpoint);
			node.Add("offset", Offset);
			return node;
		}

		public int Bindpoint { get; set; }

		public int Offset { get; set; }
	}
}
