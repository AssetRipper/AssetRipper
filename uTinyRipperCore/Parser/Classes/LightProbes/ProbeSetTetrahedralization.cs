using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.Classes.LightProbess
{
	public struct ProbeSetTetrahedralization : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_tetrahedra = reader.ReadArray<Tetrahedron>();
			m_hullRays = reader.ReadArray<Vector3f>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Tetrahedra", Tetrahedra.ExportYAML(container));
			node.Add("m_HullRays", HullRays.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<Tetrahedron> Tetrahedra => m_tetrahedra;
		public IReadOnlyList<Vector3f> HullRays => m_hullRays;

		private Tetrahedron[] m_tetrahedra;
		private Vector3f[] m_hullRays;
	}
}
