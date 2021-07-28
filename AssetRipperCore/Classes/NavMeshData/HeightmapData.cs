using AssetRipper.Project;
using AssetRipper.Parser.Asset;
using AssetRipper.Classes.Misc;
using AssetRipper.Classes.Misc.Serializable;
using AssetRipper.IO.Asset;
using AssetRipper.YAML;
using System.Collections.Generic;
using AssetRipper.Math;

namespace AssetRipper.Classes.NavMeshData
{
	public struct HeightmapData : IAssetReadable, IYAMLExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			Position.Read(reader);
			TerrainData.Read(reader);
		}

		public IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(TerrainData, TerrainDataName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(PositionName, Position.ExportYAML(container));
			node.Add(TerrainDataName, TerrainData.ExportYAML(container));
			return node;
		}

		public const string PositionName = "position";
		public const string TerrainDataName = "terrainData";

		public Vector3f Position;
		public PPtr<Object.Object> TerrainData;
	}
}
