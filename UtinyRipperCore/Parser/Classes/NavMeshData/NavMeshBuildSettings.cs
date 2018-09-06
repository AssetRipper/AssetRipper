using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.NavMeshDatas
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
			
			// min version is 2nd
			return 2;
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
			ManualCellSize = reader.ReadInt32();
			CellSize = reader.ReadSingle();
			ManualTileSize = reader.ReadInt32();
			TileSize = reader.ReadInt32();
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
			node.Add("agentTypeID", AgentTypeID);
			node.Add("agentRadius", AgentRadius);
			node.Add("agentHeight", AgentHeight);
			node.Add("agentSlope", AgentSlope);
			node.Add("agentClimb", AgentClimb);
			node.Add("ledgeDropHeight", LedgeDropHeight);
			node.Add("maxJumpAcrossDistance", MaxJumpAcrossDistance);
			node.Add("minRegionArea", MinRegionArea);
			node.Add("manualCellSize", ManualCellSize);
			node.Add("cellSize", CellSize);
			node.Add("manualTileSize", ManualTileSize);
			node.Add("tileSize", TileSize);
			node.Add("accuratePlacement", AccuratePlacement);
			node.Add("debug", Debug.ExportYAML(container));
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

		public NavMeshBuildDebugSettings Debug;
	}
}
