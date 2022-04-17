using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Avatar
{
	public sealed class Handle : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			X.Read(reader);
			ParentHumanIndex = reader.ReadUInt32();
			ID = reader.ReadUInt32();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(XName, X.ExportYaml(container));
			node.Add(ParentHumanIndexName, ParentHumanIndex);
			node.Add(IDName, ID);
			return node;
		}

		public uint ParentHumanIndex { get; set; }
		public uint ID { get; set; }

		public const string XName = "m_X";
		public const string ParentHumanIndexName = "m_ParentHumanIndex";
		public const string IDName = "m_ID";

		public XForm X = new();
	}
}
