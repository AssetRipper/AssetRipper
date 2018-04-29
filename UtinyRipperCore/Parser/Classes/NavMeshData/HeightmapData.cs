using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.NavMeshDatas
{
	public struct HeightmapData : IAssetReadable, IYAMLExportable, IDependent
	{
		public void Read(AssetStream stream)
		{
			Position.Read(stream);
			TerrainData.Read(stream);
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return TerrainData.FetchDependency(file, isLog, () => nameof(HeightmapData), "terrainData");
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("position", Position.ExportYAML(exporter));
			node.Add("terrainData", TerrainData.ExportYAML(exporter));
			return node;
		}

		public Vector3f Position;
		public PPtr<Object> TerrainData;
	}
}
