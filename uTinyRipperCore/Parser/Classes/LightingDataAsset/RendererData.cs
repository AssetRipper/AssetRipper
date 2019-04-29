using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.LightingDataAssets
{
	public struct RendererData : IAssetReadable, IYAMLExportable, IDependent
	{
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool IsReadExplicitProbeSetHash(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}

		public void Read(AssetReader reader)
		{
			UVMesh.Read(reader);
			TerrainDynamicUVST.Read(reader);
			TerrainChunkDynamicUVST.Read(reader);
			LightmapIndex = reader.ReadUInt16();
			LightmapIndexDynamic = reader.ReadUInt16();
			LightmapST.Read(reader);
			LightmapSTDynamic.Read(reader);
			if (IsReadExplicitProbeSetHash(reader.Version))
			{
				ExplicitProbeSetHash.Read(reader);
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return UVMesh.FetchDependency(file, isLog, () => nameof(RendererData), UvMeshName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(UvMeshName, UVMesh.ExportYAML(container));
			node.Add(TerrainDynamicUVSTName, TerrainDynamicUVST.ExportYAML(container));
			node.Add(TerrainChunkDynamicUVSTName, TerrainChunkDynamicUVST.ExportYAML(container));
			node.Add(LightmapIndexName, LightmapIndex);
			node.Add(LightmapIndexDynamicName, LightmapIndexDynamic);
			node.Add(LightmapSTName, LightmapST.ExportYAML(container));
			node.Add(LightmapSTDynamicName, LightmapSTDynamic.ExportYAML(container));
			if (IsReadExplicitProbeSetHash(container.ExportVersion))
			{
				node.Add(ExplicitProbeSetHashName, ExplicitProbeSetHash.ExportYAML(container));
			}
			return node;
		}

		public ushort LightmapIndex { get; private set; }
		public ushort LightmapIndexDynamic { get; private set; }

		public const string UvMeshName = "uvMesh";
		public const string TerrainDynamicUVSTName = "terrainDynamicUVST";
		public const string TerrainChunkDynamicUVSTName = "terrainChunkDynamicUVST";
		public const string LightmapIndexName = "lightmapIndex";
		public const string LightmapIndexDynamicName = "lightmapIndexDynamic";
		public const string LightmapSTName = "lightmapST";
		public const string LightmapSTDynamicName = "lightmapSTDynamic";
		public const string ExplicitProbeSetHashName = "explicitProbeSetHash";

		public PPtr<Mesh> UVMesh;
		public Vector4f TerrainDynamicUVST;
		public Vector4f TerrainChunkDynamicUVST;
		public Vector4f LightmapST;
		public Vector4f LightmapSTDynamic;
		public Hash128 ExplicitProbeSetHash;
	}
}
