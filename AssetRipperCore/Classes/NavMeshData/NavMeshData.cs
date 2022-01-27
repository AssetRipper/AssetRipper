using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Misc.Serializable.Boundaries;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Core.Classes.NavMeshData
{
	/// <summary>
	/// Successor of NavMesh
	/// </summary>
	public sealed class NavMeshData : NamedObject
	{
		public NavMeshData(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(5, 6))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		public static bool HasNavMeshParams(UnityVersion version) => version.IsLess(5, 6);
		/// <summary>
		/// 5.6.0p1 and greater
		/// </summary>
		public static bool HasSourceBounds(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Patch);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			NavMeshTiles = reader.ReadAssetArray<NavMeshTileData>();
			if (HasNavMeshParams(reader.Version))
			{
				NavMeshParams.Read(reader);
			}
			else
			{
				NavMeshBuildSettings.Read(reader);
			}
			Heightmaps = reader.ReadAssetArray<HeightmapData>();
			HeightMeshes = reader.ReadAssetArray<HeightMeshData>();
			OffMeshLinks = reader.ReadAssetArray<AutoOffMeshLinkData>();
			if (HasSourceBounds(reader.Version))
			{
				SourceBounds.Read(reader);
				Rotation.Read(reader);
				Position.Read(reader);
				AgentTypeID = reader.ReadInt32();
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromArray(Heightmaps, HeightmapsName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.InsertSerializedVersion(ToSerializedVersion(container.ExportVersion));
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

		private NavMeshBuildSettings GetExportNavMeshBuildSettings(UnityVersion version)
		{
			return HasNavMeshParams(version) ? new NavMeshBuildSettings(NavMeshParams) : NavMeshBuildSettings;
		}
		private Quaternionf GetExportRotation(UnityVersion version)
		{
			return HasSourceBounds(version) ? Rotation : Quaternionf.Zero;
		}

		public override string ExportPath => Path.Combine(AssetsKeyword, OcclusionCullingSettings.OcclusionCullingSettings.SceneKeyword, ClassID.ToString());

		public NavMeshTileData[] NavMeshTiles { get; set; }
		public HeightmapData[] Heightmaps { get; set; }
		public HeightMeshData[] HeightMeshes { get; set; }
		public AutoOffMeshLinkData[] OffMeshLinks { get; set; }
		public int AgentTypeID { get; set; }

		public const string NavMeshTilesName = "m_NavMeshTiles";
		public const string NavMeshBuildSettingsName = "m_NavMeshBuildSettings";
		public const string HeightmapsName = "m_Heightmaps";
		public const string HeightMeshesName = "m_HeightMeshes";
		public const string OffMeshLinksName = "m_OffMeshLinks";
		public const string SourceBoundsName = "m_SourceBounds";
		public const string RotationName = "m_Rotation";
		public const string PositionName = "m_Position";
		public const string AgentTypeIDName = "m_AgentTypeID";

		public NavMeshParams NavMeshParams = new();
		public NavMeshBuildSettings NavMeshBuildSettings = new();
		public AABB SourceBounds = new();
		public Quaternionf Rotation = new();
		public Vector3f Position = new();
	}
}
