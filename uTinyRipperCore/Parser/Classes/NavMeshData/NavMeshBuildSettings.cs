using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.NavMeshDatas
{
	public struct NavMeshBuildSettings : IAssetReadable, IYAMLExportable
	{
		public NavMeshBuildSettings(bool _)
		{
			AgentTypeID = 0;
			AgentRadius = 0.5f;
			AgentHeight = 2.0f;
			AgentSlope = 45.0f;
			AgentClimb = 0.4f;
			LedgeDropHeight = 0.0f;
			MaxJumpAcrossDistance = 0.0f;
			MinRegionArea = 2.0f;
			ManualCellSize = 0;
			CellSize = 1.0f / 6.0f;
			ManualTileSize = 0;
			TileSize = 256;
			AccuratePlacement = 0;
			Debug = default;
		}

		public NavMeshBuildSettings(float agentClimb, float cellSize) :
			this(true)
		{
			AgentClimb = agentClimb;
			ManualCellSize = 1;
			CellSize = cellSize;
		}

		public NavMeshBuildSettings(NavMeshParams navParams) :
			this(true)
		{
			AgentRadius = navParams.WalkableRadius;
			AgentHeight = navParams.WalkableHeight;
			AgentClimb = navParams.WalkableClimb;
			TileSize = (int)navParams.TileSize;
			CellSize = navParams.CellSize;
		}

		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool IsReadDebug(Version version)
		{
			return version.IsGreaterEqual(2017, 2);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}
			return 2;
			// 5.6.0.Alpha.unknown
			//return 1;
		}

		public void Read(AssetReader reader)
		{
			AgentTypeID = reader.ReadInt32();
			AgentRadius = reader.ReadSingle();
			AgentHeight = reader.ReadSingle();
			AgentSlope = reader.ReadSingle();
			AgentClimb = reader.ReadSingle();
			LedgeDropHeight = reader.ReadSingle();
			MaxJumpAcrossDistance = reader.ReadSingle();
			MinRegionArea = reader.ReadSingle();
			// it is bool with align in 5.6 beta but there is no difference
			ManualCellSize = reader.ReadInt32();
			CellSize = reader.ReadSingle();
			ManualTileSize = reader.ReadInt32();
			TileSize = reader.ReadInt32();
			// it is bool with align in 5.6 beta but there is no difference
			AccuratePlacement = reader.ReadInt32();
			if (IsReadDebug(reader.Version))
			{
				Debug.Read(reader);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add(AgentTypeIDName, AgentTypeID);
			node.Add(AgentRadiusName, AgentRadius);
			node.Add(AgentHeightName, AgentHeight);
			node.Add(AgentSlopeName, AgentSlope);
			node.Add(AgentClimbName, AgentClimb);
			node.Add(LedgeDropHeightName, LedgeDropHeight);
			node.Add(MaxJumpAcrossDistanceName, MaxJumpAcrossDistance);
			node.Add(MinRegionAreaName, MinRegionArea);
			node.Add(ManualCellSizeName, ManualCellSize);
			node.Add(CellSizeName, CellSize);
			node.Add(ManualTileSizeName, ManualTileSize);
			node.Add(TileSizeName, TileSize);
			node.Add(AccuratePlacementName, AccuratePlacement);
			node.Add(DebugName, Debug.ExportYAML(container));
			return node;

		}

		public int AgentTypeID { get; set; }
		public float AgentRadius { get; set; }
		public float AgentHeight { get; set; }
		public float AgentSlope { get; set; }
		public float AgentClimb { get; set; }
		public float LedgeDropHeight { get; set; }
		public float MaxJumpAcrossDistance { get; set; }
		public float MinRegionArea { get; set; }
		public int ManualCellSize { get; set; }
		public float CellSize { get; set; }
		public int ManualTileSize { get; set; }
		public int TileSize { get; set; }
		public int AccuratePlacement { get; set; }

		public const string AgentTypeIDName = "agentTypeID";
		public const string AgentRadiusName = "agentRadius";
		public const string AgentHeightName = "agentHeight";
		public const string AgentSlopeName = "agentSlope";
		public const string AgentClimbName = "agentClimb";
		public const string LedgeDropHeightName = "ledgeDropHeight";
		public const string MaxJumpAcrossDistanceName = "maxJumpAcrossDistance";
		public const string MinRegionAreaName = "minRegionArea";
		public const string ManualCellSizeName = "manualCellSize";
		public const string CellSizeName = "cellSize";
		public const string ManualTileSizeName = "manualTileSize";
		public const string TileSizeName = "tileSize";
		public const string AccuratePlacementName = "accuratePlacement";
		public const string DebugName = "debug";

		public NavMeshBuildDebugSettings Debug;
	}
}
