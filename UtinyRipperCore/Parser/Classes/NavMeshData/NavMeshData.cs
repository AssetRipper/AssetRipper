using System.Collections.Generic;
using System.IO;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.NavMeshDatas;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public sealed class NavMeshData : NamedObject
	{
		public NavMeshData(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		public static bool IsReadNavMeshParams(Version version)
		{
			return version.IsLess(5, 6);
		}
		/// <summary>
		/// 5.6.1 and greater
		/// </summary>
		public static bool IsReadSourceBounds(Version version)
		{
			return version.IsGreaterEqual(5, 6, 1);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			if (version.IsGreaterEqual(5, 6))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_navMeshTiles = reader.ReadArray<NavMeshTileData>();
			if (IsReadNavMeshParams(reader.Version))
			{
				NavMeshParams.Read(reader);
			}
			else
			{
				NavMeshBuildSettings.Read(reader);
			}
			m_heightmaps = reader.ReadArray<HeightmapData>();
			m_heightMeshes = reader.ReadArray<HeightMeshData>();
			m_offMeshLinks = reader.ReadArray<AutoOffMeshLinkData>();
			if (IsReadSourceBounds(reader.Version))
			{
				SourceBounds.Read(reader);
				Rotation.Read(reader);
				Position.Read(reader);
				AgentTypeID = reader.ReadInt32();
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object dependency in base.FetchDependencies(file, isLog))
			{
				yield return dependency;
			}

			foreach (HeightmapData heightmap in Heightmaps)
			{
				foreach (Object dependency in heightmap.FetchDependencies(file, isLog))
				{
					yield return dependency;
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.InsertSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_NavMeshTiles", NavMeshTiles.ExportYAML(container));
			node.Add("m_NavMeshBuildSettings", GetExportNavMeshBuildSettings(container.Version).ExportYAML(container));
			node.Add("m_Heightmaps", Heightmaps.ExportYAML(container));
			node.Add("m_HeightMeshes", HeightMeshes.ExportYAML(container));
			node.Add("m_OffMeshLinks", OffMeshLinks.ExportYAML(container));
			node.Add("m_SourceBounds", SourceBounds.ExportYAML(container));
			node.Add("m_Rotation", GetExportRotation(container.Version).ExportYAML(container));
			node.Add("m_Position", Position.ExportYAML(container));
			node.Add("m_AgentTypeID", AgentTypeID);
			return node;
		}

		private NavMeshBuildSettings GetExportNavMeshBuildSettings(Version version)
		{
			return IsReadNavMeshParams(version) ? new NavMeshBuildSettings(NavMeshParams) : NavMeshBuildSettings;
		}
		private Quaternionf GetExportRotation(Version version)
		{
			return IsReadSourceBounds(version) ? Rotation : Quaternionf.Zero;
		}

		public override string ExportName => Path.Combine(AssetsKeyWord, OcclusionCullingSettings.SceneKeyWord, ClassID.ToString());

		public IReadOnlyList<NavMeshTileData> NavMeshTiles => m_navMeshTiles;
		public IReadOnlyList<HeightmapData> Heightmaps => m_heightmaps;
		public IReadOnlyList<HeightMeshData> HeightMeshes => m_heightMeshes;
		public IReadOnlyList<AutoOffMeshLinkData> OffMeshLinks => m_offMeshLinks;
		public int AgentTypeID { get; private set; }

		public NavMeshParams NavMeshParams;
		public NavMeshBuildSettings NavMeshBuildSettings;
		public AABB SourceBounds;
		public Quaternionf Rotation;
		public Vector3f Position;

		private NavMeshTileData[] m_navMeshTiles;
		private HeightmapData[] m_heightmaps;
		private HeightMeshData[] m_heightMeshes;
		private AutoOffMeshLinkData[] m_offMeshLinks;
	}
}
