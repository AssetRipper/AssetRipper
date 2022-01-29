using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;

namespace AssetRipper.Core.Classes.Misc
{
	public sealed class FixedBitset : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Data = reader.ReadUInt32Array();
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(DataName, Data.ExportYAML(true));
			return node;
		}

		public uint[] Data { get; set; }

		public const string DataName = "data";
	}
}
