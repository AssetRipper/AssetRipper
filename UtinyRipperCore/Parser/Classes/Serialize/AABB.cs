using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public struct AABB : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			Center.Read(stream);
			Extent.Read(stream);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Center", Center.ExportYAML(exporter));
			node.Add("m_Extent", Extent.ExportYAML(exporter));
			return node;
		}

		public Vector3f Center;
		public Vector3f Extent;
	}
}
