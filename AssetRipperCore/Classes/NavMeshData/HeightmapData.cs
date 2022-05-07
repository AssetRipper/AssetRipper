using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.NavMeshData
{
	public sealed class HeightmapData : IAssetReadable, IYamlExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			Position.Read(reader);
			TerrainData.Read(reader);
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(TerrainData, TerrainDataName);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(PositionName, Position.ExportYaml(container));
			node.Add(TerrainDataName, TerrainData.ExportYaml(container));
			return node;
		}

		public const string PositionName = "position";
		public const string TerrainDataName = "terrainData";

		public Vector3f Position = new();
		public PPtr<Object.Object> TerrainData = new();
	}
}
