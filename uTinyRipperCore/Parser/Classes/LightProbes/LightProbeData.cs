using System.Collections.Generic;
using uTinyRipper.Project;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Classes.LightProbess
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
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool IsReadNonTetrahedralizedProbeSetIndexMap(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		
		public void Read(AssetReader reader)
		{
			Tetrahedralization.Read(reader);
			if (IsReadProbeSets(reader.Version))
			{
				m_probeSets = reader.ReadAssetArray<ProbeSetIndex>();
				m_positions = reader.ReadAssetArray<Vector3f>();
			}
			if (IsReadNonTetrahedralizedProbeSetIndexMap(reader.Version))
			{
				m_nonTetrahedralizedProbeSetIndexMap = new Dictionary<Hash128, int>();
				m_nonTetrahedralizedProbeSetIndexMap.Read(reader);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO:
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TetrahedralizationName, Tetrahedralization.ExportYAML(container));
			node.Add(ProbeSetsName, IsReadProbeSets(container.Version) ? ProbeSets.ExportYAML(container) : YAMLSequenceNode.Empty);
			node.Add(PositionsName, IsReadProbeSets(container.Version) ? Positions.ExportYAML(container) : YAMLSequenceNode.Empty);
			node.Add(NonTetrahedralizedProbeSetIndexMapName, IsReadNonTetrahedralizedProbeSetIndexMap(container.Version) ? NonTetrahedralizedProbeSetIndexMap.ExportYAML(container) : YAMLSequenceNode.Empty);
			return node;
		}

		public IReadOnlyList<ProbeSetIndex> ProbeSets => m_probeSets;
		public IReadOnlyList<Vector3f> Positions => m_positions;
		public IReadOnlyDictionary<Hash128, int> NonTetrahedralizedProbeSetIndexMap => m_nonTetrahedralizedProbeSetIndexMap;

		public const string TetrahedralizationName = "m_Tetrahedralization";
		public const string ProbeSetsName = "m_ProbeSets";
		public const string PositionsName = "m_Positions";
		public const string NonTetrahedralizedProbeSetIndexMapName = "m_NonTetrahedralizedProbeSetIndexMap";

		public ProbeSetTetrahedralization Tetrahedralization;

		private ProbeSetIndex[] m_probeSets;
		private Vector3f[] m_positions;
		private Dictionary<Hash128, int> m_nonTetrahedralizedProbeSetIndexMap;
	}
}
