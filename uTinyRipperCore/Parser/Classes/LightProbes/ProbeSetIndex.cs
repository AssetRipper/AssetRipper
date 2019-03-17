using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.LightProbess
{
	public struct ProbeSetIndex : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Hash.Read(reader);
			Offset = reader.ReadInt32();
			Size = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Hash", Hash.ExportYAML(container));
			node.Add("m_Offset", Offset);
			node.Add("m_Size", Size);
			return node;
		}

		public int Offset { get; private set; }
		public int Size { get; private set; }

		public Hash128 Hash;
	}
}
