using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.NavMeshDatas
{
	/// <summary>
	/// Introduced in 5.6.0
	/// </summary>
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

		public static int ToSerializedVersion(Version version)
		{
			return 2;
			// NOTE: unknown version (5.6.0a)
			//return 1;
		}

		/// <summary>
		/// 5.6.0bx
		/// </summary>
		public static bool IsBoolFlags(Version version) => version.IsEqual(5, 6, 0, VersionType.Beta);
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasDebug(Version version) => version.IsGreaterEqual(2017, 2);

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
			if (IsBoolFlags(reader.Version))
			{
				ManualCellSizeBool = reader.ReadBoolean();
				reader.AlignStream();
			}
			else
			{
				ManualCellSize = reader.ReadInt32();
			}
			CellSize = reader.ReadSingle();
			if (IsBoolFlags(reader.Version))
			{
				ManualTileSizeBool = reader.ReadBoolean();
				reader.AlignStream();
			}
			else
			{
				ManualTileSize = reader.ReadInt32();
			}
			TileSize = reader.ReadInt32();
			if (IsBoolFlags(reader.Version))
			{
				AccuratePlacementBool = reader.ReadBoolean();
				reader.AlignStream();
			}
			else
			{
				AccuratePlacement = reader.ReadInt32();
			}
			if (HasDebug(reader.Version))
			{
				Debug.Read(reader);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
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
		public bool ManualCellSizeBool
		{
			get => ManualCellSize != 0;
			set => ManualCellSize = value ? 1 : 0;
		}
		public int ManualCellSize { get; set; }
		public float CellSize { get; set; }
		public bool ManualTileSizeBool
		{
			get => ManualTileSize != 0;
			set => ManualTileSize = value ? 1 : 0;
		}
		public int ManualTileSize { get; set; }
		public int TileSize { get; set; }
		public bool AccuratePlacementBool
		{
			get => AccuratePlacement != 0;
			set => AccuratePlacement = value ? 1 : 0;
		}
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
