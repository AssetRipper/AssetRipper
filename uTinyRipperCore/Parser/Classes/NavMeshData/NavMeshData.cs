using System.Collections.Generic;
using System.IO;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.NavMeshDatas;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// Successor of NavMesh
	/// </summary>
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
			if (version.IsGreaterEqual(5, 6))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_navMeshTiles = reader.ReadAssetArray<NavMeshTileData>();
			if (IsReadNavMeshParams(reader.Version))
			{
				NavMeshParams.Read(reader);
			}
			else
			{
				NavMeshBuildSettings.Read(reader);
			}
			m_heightmaps = reader.ReadAssetArray<HeightmapData>();
			m_heightMeshes = reader.ReadAssetArray<HeightMeshData>();
			m_offMeshLinks = reader.ReadAssetArray<AutoOffMeshLinkData>();
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
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.InsertSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(NavMeshTilesName, NavMeshTiles.ExportYAML(container));
			node.Add(NavMeshBuildSettingsName, GetExportNavMeshBuildSettings(container.Version).ExportYAML(container));
			node.Add(HeightmapsName, Heightmaps.ExportYAML(container));
			node.Add(HeightMeshesName, HeightMeshes.ExportYAML(container));
			node.Add(OffMeshLinksName, OffMeshLinks.ExportYAML(container));
			node.Add(SourceBoundsName, SourceBounds.ExportYAML(container));
			node.Add(RotationName, GetExportRotation(container.Version).ExportYAML(container));
			node.Add(PositionName, Position.ExportYAML(container));
			node.Add(AgentTypeIDName, AgentTypeID);
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

		public override string ExportPath => Path.Combine(AssetsKeyword, OcclusionCullingSettings.SceneKeyword, ClassID.ToString());

		public IReadOnlyList<NavMeshTileData> NavMeshTiles => m_navMeshTiles;
		public IReadOnlyList<HeightmapData> Heightmaps => m_heightmaps;
		public IReadOnlyList<HeightMeshData> HeightMeshes => m_heightMeshes;
		public IReadOnlyList<AutoOffMeshLinkData> OffMeshLinks => m_offMeshLinks;
		public int AgentTypeID { get; private set; }

		public const string NavMeshTilesName = "m_NavMeshTiles";
		public const string NavMeshBuildSettingsName = "m_NavMeshBuildSettings";
		public const string HeightmapsName = "m_Heightmaps";
		public const string HeightMeshesName = "m_HeightMeshes";
		public const string OffMeshLinksName = "m_OffMeshLinks";
		public const string SourceBoundsName = "m_SourceBounds";
		public const string RotationName = "m_Rotation";
		public const string PositionName = "m_Position";
		public const string AgentTypeIDName = "m_AgentTypeID";

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
