using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.PolygonCollider2Ds
{
	public struct Polygon2D : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			m_paths = stream.ReadArrayDouble<Vector2f>();
			stream.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Paths", m_paths.ExportYAML(exporter));
			return node;
		}

		public IReadOnlyList<IReadOnlyList<Vector2f>> Paths => m_paths;

		private Vector2f[][] m_paths;
	}
}
