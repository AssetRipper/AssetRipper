using System.Collections.Generic;
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
		public static bool HasProbeSets(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool HasNonTetrahedralizedProbeSetIndexMap(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		
		public void Read(AssetReader reader)
		{
			Tetrahedralization.Read(reader);
			if (HasProbeSets(reader.Version))
			{
				ProbeSets = reader.ReadAssetArray<ProbeSetIndex>();
				Positions = reader.ReadAssetArray<Vector3f>();
			}
			if (HasNonTetrahedralizedProbeSetIndexMap(reader.Version))
			{
				NonTetrahedralizedProbeSetIndexMap = new Dictionary<Hash128, int>();
				NonTetrahedralizedProbeSetIndexMap.Read(reader);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO:
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TetrahedralizationName, Tetrahedralization.ExportYAML(container));
			node.Add(ProbeSetsName, HasProbeSets(container.Version) ? ProbeSets.ExportYAML(container) : YAMLSequenceNode.Empty);
			node.Add(PositionsName, HasProbeSets(container.Version) ? Positions.ExportYAML(container) : YAMLSequenceNode.Empty);
			node.Add(NonTetrahedralizedProbeSetIndexMapName, HasNonTetrahedralizedProbeSetIndexMap(container.Version) ? NonTetrahedralizedProbeSetIndexMap.ExportYAML(container) : YAMLSequenceNode.Empty);
			return node;
		}

		public ProbeSetIndex[] ProbeSets { get; set; }
		public Vector3f[] Positions { get; set; }
		public Dictionary<Hash128, int> NonTetrahedralizedProbeSetIndexMap { get; set; }

		public const string TetrahedralizationName = "m_Tetrahedralization";
		public const string ProbeSetsName = "m_ProbeSets";
		public const string PositionsName = "m_Positions";
		public const string NonTetrahedralizedProbeSetIndexMapName = "m_NonTetrahedralizedProbeSetIndexMap";

		public ProbeSetTetrahedralization Tetrahedralization;
	}
}
