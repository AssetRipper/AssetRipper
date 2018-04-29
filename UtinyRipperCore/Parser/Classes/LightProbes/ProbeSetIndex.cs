using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.LightProbess
{
	public struct ProbeSetIndex : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			Hash.Read(stream);
			Offset = stream.ReadInt32();
			Size = stream.ReadInt32();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Hash", Hash.ExportYAML(exporter));
			node.Add("m_Offset", Offset);
			node.Add("m_Size", Size);
			return node;
		}

		public int Offset { get; private set; }
		public int Size { get; private set; }

		public Hash128 Hash;
	}
}
