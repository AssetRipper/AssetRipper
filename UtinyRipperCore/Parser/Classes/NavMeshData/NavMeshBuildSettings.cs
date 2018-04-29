using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.NavMeshDatas
{
	public struct NavMeshBuildSettings : IAssetReadable, IYAMLExportable
	{
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

		public void Read(AssetStream stream)
		{
			AgentTypeID = stream.ReadInt32();
			AgentRadius = stream.ReadSingle();
			AgentHeight = stream.ReadSingle();
			AgentSlope = stream.ReadSingle();
			AgentClimb = stream.ReadSingle();
			LedgeDropHeight = stream.ReadSingle();
			MaxJumpAcrossDistance = stream.ReadSingle();
			MinRegionArea = stream.ReadSingle();
			ManualCellSize = stream.ReadInt32();
			CellSize = stream.ReadSingle();
			ManualTileSize = stream.ReadInt32();
			TileSize = stream.ReadInt32();
			AccuratePlacement = stream.ReadInt32();
			if (IsReadDebug(stream.Version))
			{
				Debug.Read(stream);
			}
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
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
			node.Add("debug", Debug.ExportYAML(exporter));
			return node;
		}

		public int AgentTypeID { get; private set; }
		public float AgentRadius { get; private set; }
		public float AgentHeight { get; private set; }
		public float AgentSlope { get; private set; }
		public float AgentClimb { get; private set; }
		public float LedgeDropHeight { get; private set; }
		public float MaxJumpAcrossDistance { get; private set; }
		public float MinRegionArea { get; private set; }
		public int ManualCellSize { get; private set; }
		public float CellSize { get; private set; }
		public int ManualTileSize { get; private set; }
		public int TileSize { get; private set; }
		public int AccuratePlacement { get; private set; }

		public NavMeshBuildDebugSettings Debug;
	}
}
