using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.LightProbess
{
	public struct ProbeSetTetrahedralization : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_tetrahedra = reader.ReadAssetArray<Tetrahedron>();
			m_hullRays = reader.ReadAssetArray<Vector3f>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TetrahedraName, Tetrahedra.ExportYAML(container));
			node.Add(HullRaysName, HullRays.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<Tetrahedron> Tetrahedra => m_tetrahedra;
		public IReadOnlyList<Vector3f> HullRays => m_hullRays;

		public const string TetrahedraName = "m_Tetrahedra";
		public const string HullRaysName = "m_HullRays";

		private Tetrahedron[] m_tetrahedra;
		private Vector3f[] m_hullRays;
	}
}
