using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.NavMeshDatas
{
	public struct NavMeshBuildDebugSettings : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			Flags = stream.ReadByte();
			stream.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Flags", Flags);
			return node;
		}

		public byte Flags { get; private set; }
	}
}
