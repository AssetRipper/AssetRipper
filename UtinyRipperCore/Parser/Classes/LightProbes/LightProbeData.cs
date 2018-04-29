using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.LightProbess
{
	public struct LightProbeData : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadProbeSets(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// Greater than 5.0.0b1
		/// </summary>
		public static bool IsReadNonTetrahedralizedProbeSetIndexMap(Version version)
		{
#warning unknown
			return version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 1);
		}
		
		public void Read(AssetStream stream)
		{
			Tetrahedralization.Read(stream);
			if(IsReadProbeSets(stream.Version))
			{
				m_probeSets = stream.ReadArray<ProbeSetIndex>();
				m_positions = stream.ReadArray<Vector3f>();
			}
			if(IsReadNonTetrahedralizedProbeSetIndexMap(stream.Version))
			{
				m_nonTetrahedralizedProbeSetIndexMap.Read(stream);
			}
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Tetrahedralization", Tetrahedralization.ExportYAML(exporter));
			node.Add("m_ProbeSets", IsReadProbeSets(exporter.Version) ? ProbeSets.ExportYAML(exporter) : YAMLSequenceNode.Empty);
			node.Add("m_Positions", IsReadProbeSets(exporter.Version) ? Positions.ExportYAML(exporter) : YAMLSequenceNode.Empty);
			node.Add("m_NonTetrahedralizedProbeSetIndexMap", IsReadNonTetrahedralizedProbeSetIndexMap(exporter.Version) ? NonTetrahedralizedProbeSetIndexMap.ExportYAML(exporter) : YAMLSequenceNode.Empty);
			return node;
		}

		public IReadOnlyList<ProbeSetIndex> ProbeSets => m_probeSets;
		public IReadOnlyList<Vector3f> Positions => m_positions;
		public IReadOnlyDictionary<Hash128, int> NonTetrahedralizedProbeSetIndexMap => m_nonTetrahedralizedProbeSetIndexMap;

		public ProbeSetTetrahedralization Tetrahedralization;

		private ProbeSetIndex[] m_probeSets;
		private Vector3f[] m_positions;
		private Dictionary<Hash128, int> m_nonTetrahedralizedProbeSetIndexMap;
	}
}
