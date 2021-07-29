using AssetRipper.Project;
using AssetRipper.Classes.Misc;
using AssetRipper.IO.Asset;
using AssetRipper.YAML;

namespace AssetRipper.Classes.Avatar
{
	public class Handle : IAssetReadable, IYAMLExportable
	{
		public Handle() { }
		public void Read(AssetReader reader)
		{
			X.Read(reader);
			ParentHumanIndex = reader.ReadUInt32();
			ID = reader.ReadUInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(XName, X.ExportYAML(container));
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
