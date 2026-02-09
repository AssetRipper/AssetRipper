using AssetRipper.SourceGenerated.Subclasses.NavMeshBuildSettings;
using AssetRipper.SourceGenerated.Subclasses.NavMeshParams;

namespace AssetRipper.SourceGenerated.Extensions;

public static class NavMeshBuildSettingsExtensions
{
	public static void SetToDefault(this INavMeshBuildSettings settings)
	{
		settings.AgentTypeID = 0;
		settings.AgentRadius = 0.5f;
		settings.AgentHeight = 2.0f;
		settings.AgentSlope = 45.0f;
		settings.AgentClimb = 0.4f;
		settings.LedgeDropHeight = 0.0f;
		settings.MaxJumpAcrossDistance = 0.0f;
		settings.MinRegionArea = 2.0f;
		settings.ManualCellSize_Int32 = 0;
		settings.ManualCellSize_Boolean = false;
		settings.CellSize = 1.0f / 6.0f;
		settings.ManualTileSize_Int32 = 0;
		settings.ManualTileSize_Boolean = false;
		settings.TileSize = 256;
		settings.AccuratePlacement_Int32 = 0;
		settings.AccuratePlacement_Boolean = false;
		settings.MaxJobWorkers = 0;
		settings.PreserveTilesOutsideBounds = 0;
	}

	public static void SetValues(this INavMeshBuildSettings settings, float agentClimb, float cellSize)
	{
		settings.AgentClimb = agentClimb;
		settings.ManualCellSize_Int32 = 1;
		settings.ManualCellSize_Boolean = true;
		settings.CellSize = cellSize;
	}

	public static void SetValues(this INavMeshBuildSettings settings, INavMeshParams navParams)
	{
		settings.SetToDefault();
		settings.AgentRadius = navParams.WalkableRadius;
		settings.AgentHeight = navParams.WalkableHeight;
		settings.AgentClimb = navParams.WalkableClimb;
		settings.TileSize = (int)navParams.TileSize;
		settings.CellSize = navParams.CellSize;
	}
}
