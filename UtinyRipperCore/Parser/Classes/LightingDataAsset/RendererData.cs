using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.LightingDataAssets
{
	public struct RendererData : IAssetReadable, IYAMLExportable, IDependent
	{
		public void Read(AssetStream stream)
		{
			UVMesh.Read(stream);
			TerrainDynamicUVST.Read(stream);
			TerrainChunkDynamicUVST.Read(stream);
			LightmapIndex = stream.ReadUInt16();
			LightmapIndexDynamic = stream.ReadUInt16();
			LightmapST.Read(stream);
			LightmapSTDynamic.Read(stream);
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return UVMesh.FetchDependency(file, isLog, () => nameof(RendererData), "uvMesh");
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("uvMesh", UVMesh.ExportYAML(exporter));
			node.Add("terrainDynamicUVST", TerrainDynamicUVST.ExportYAML(exporter));
			node.Add("terrainChunkDynamicUVST", TerrainChunkDynamicUVST.ExportYAML(exporter));
			node.Add("lightmapIndex", LightmapIndex);
			node.Add("lightmapIndexDynamic", LightmapIndexDynamic);
			node.Add("lightmapST", LightmapST.ExportYAML(exporter));
			node.Add("lightmapSTDynamic", LightmapSTDynamic.ExportYAML(exporter));
			return node;
		}

		public ushort LightmapIndex { get; private set; }
		public ushort LightmapIndexDynamic { get; private set; }

		public PPtr<Mesh> UVMesh;
		public Vector4f TerrainDynamicUVST;
		public Vector4f TerrainChunkDynamicUVST;
		public Vector4f LightmapST;
		public Vector4f LightmapSTDynamic;
	}
}
