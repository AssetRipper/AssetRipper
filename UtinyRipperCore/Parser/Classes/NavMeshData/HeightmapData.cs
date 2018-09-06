using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.NavMeshDatas
{
	public struct HeightmapData : IAssetReadable, IYAMLExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			Position.Read(reader);
			TerrainData.Read(reader);
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return TerrainData.FetchDependency(file, isLog, () => nameof(HeightmapData), "terrainData");
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("position", Position.ExportYAML(container));
			node.Add("terrainData", TerrainData.ExportYAML(container));
			return node;
		}

		public Vector3f Position;
		public PPtr<Object> TerrainData;
	}
}
