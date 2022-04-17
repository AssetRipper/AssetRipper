using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.CompositeCollider2D
{
	public sealed class IntPoint : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			X = reader.ReadInt64();
			Y = reader.ReadInt64();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(XName, X);
			node.Add(YName, Y);
			return node;
		}

		public long X { get; set; }
		public long Y { get; set; }

		public const string XName = "X";
		public const string YName = "Y";
	}
}
