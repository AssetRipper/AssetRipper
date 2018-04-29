using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.LightProbess
{
	public struct ProbeSetTetrahedralization : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			m_tetrahedra = stream.ReadArray<Tetrahedron>();
			m_hullRays = stream.ReadArray<Vector3f>();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Tetrahedra", Tetrahedra.ExportYAML(exporter));
			node.Add("m_HullRays", HullRays.ExportYAML(exporter));
			return node;
		}

		public IReadOnlyList<Tetrahedron> Tetrahedra => m_tetrahedra;
		public IReadOnlyList<Vector3f> HullRays => m_hullRays;

		private Tetrahedron[] m_tetrahedra;
		private Vector3f[] m_hullRays;
	}
}
