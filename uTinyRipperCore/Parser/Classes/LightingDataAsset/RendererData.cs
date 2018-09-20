using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.LightingDataAssets
{
	public struct RendererData : IAssetReadable, IYAMLExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			UVMesh.Read(reader);
			TerrainDynamicUVST.Read(reader);
			TerrainChunkDynamicUVST.Read(reader);
			LightmapIndex = reader.ReadUInt16();
			LightmapIndexDynamic = reader.ReadUInt16();
			LightmapST.Read(reader);
			LightmapSTDynamic.Read(reader);
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return UVMesh.FetchDependency(file, isLog, () => nameof(RendererData), "uvMesh");
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("uvMesh", UVMesh.ExportYAML(container));
			node.Add("terrainDynamicUVST", TerrainDynamicUVST.ExportYAML(container));
			node.Add("terrainChunkDynamicUVST", TerrainChunkDynamicUVST.ExportYAML(container));
			node.Add("lightmapIndex", LightmapIndex);
			node.Add("lightmapIndexDynamic", LightmapIndexDynamic);
			node.Add("lightmapST", LightmapST.ExportYAML(container));
			node.Add("lightmapSTDynamic", LightmapSTDynamic.ExportYAML(container));
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
