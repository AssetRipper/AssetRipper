using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.NavMeshDatas
{
	public struct NavMeshParams : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			TileSize = reader.ReadSingle();
			WalkableHeight = reader.ReadSingle();
			WalkableRadius = reader.ReadSingle();
			WalkableClimb = reader.ReadSingle();
			CellSize = reader.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TileSizeName, TileSize);
			node.Add(WalkableHeightName, WalkableHeight);
			node.Add(WalkableRadiusName, WalkableRadius);
			node.Add(WalkableClimbName, WalkableClimb);
			node.Add(CellSizeName, CellSize);
			return node;
		}

		public float TileSize { get; private set; }
		public float WalkableHeight { get; private set; }
		public float WalkableRadius { get; private set; }
		public float WalkableClimb { get; private set; }
		public float CellSize { get; private set; }

		public const string TileSizeName = "tileSize";
		public const string WalkableHeightName = "walkableHeight";
		public const string WalkableRadiusName = "walkableRadius";
		public const string WalkableClimbName = "walkableClimb";
		public const string CellSizeName = "cellSize";
	}
}
