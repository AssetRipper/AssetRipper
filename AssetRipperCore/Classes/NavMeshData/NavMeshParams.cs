using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.NavMeshData
{
	public sealed class NavMeshParams : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			TileSize = reader.ReadSingle();
			WalkableHeight = reader.ReadSingle();
			WalkableRadius = reader.ReadSingle();
			WalkableClimb = reader.ReadSingle();
			CellSize = reader.ReadSingle();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(TileSizeName, TileSize);
			node.Add(WalkableHeightName, WalkableHeight);
			node.Add(WalkableRadiusName, WalkableRadius);
			node.Add(WalkableClimbName, WalkableClimb);
			node.Add(CellSizeName, CellSize);
			return node;
		}

		public float TileSize { get; set; }
		public float WalkableHeight { get; set; }
		public float WalkableRadius { get; set; }
		public float WalkableClimb { get; set; }
		public float CellSize { get; set; }

		public const string TileSizeName = "tileSize";
		public const string WalkableHeightName = "walkableHeight";
		public const string WalkableRadiusName = "walkableRadius";
		public const string WalkableClimbName = "walkableClimb";
		public const string CellSizeName = "cellSize";
	}
}
