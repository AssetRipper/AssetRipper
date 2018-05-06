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

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Center", Center.ExportYAML(container));
			node.Add("m_Extent", Extent.ExportYAML(container));
			return node;
		}

		public Vector3f Center;
		public Vector3f Extent;
	}
}
