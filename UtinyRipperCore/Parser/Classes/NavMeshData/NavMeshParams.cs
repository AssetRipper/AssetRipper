using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.NavMeshDatas
{
	public struct NavMeshParams : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			TileSize = stream.ReadSingle();
			WalkableHeight = stream.ReadSingle();
			WalkableRadius = stream.ReadSingle();
			WalkableClimb = stream.ReadSingle();
			CellSize = stream.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("tileSize", TileSize);
			node.Add("walkableHeight", WalkableHeight);
			node.Add("walkableRadius", WalkableRadius);
			node.Add("walkableClimb", WalkableClimb);
			node.Add("cellSize", CellSize);
			return node;
		}

		public float TileSize { get; private set; }
		public float WalkableHeight { get; private set; }
		public float WalkableRadius { get; private set; }
		public float WalkableClimb { get; private set; }
		public float CellSize { get; private set; }
	}
}
