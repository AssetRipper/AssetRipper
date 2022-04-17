using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;

namespace AssetRipper.Core.Classes.Misc
{
	public sealed class FixedBitset : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Data = reader.ReadUInt32Array();
			reader.AlignStream();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(DataName, Data.ExportYaml(true));
			return node;
		}

		public uint[] Data { get; set; }

		public const string DataName = "data";
	}
}
