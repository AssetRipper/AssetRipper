﻿using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.LightingDataAsset
{
	public sealed class RendererData : IAssetReadable, IYamlExportable, IDependent
	{
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasExplicitProbeSetHash(UnityVersion version) => version.IsGreaterEqual(2018, 2);

		public void Read(AssetReader reader)
		{
			UVMesh.Read(reader);
			TerrainDynamicUVST.Read(reader);
			TerrainChunkDynamicUVST.Read(reader);
			LightmapIndex = reader.ReadUInt16();
			LightmapIndexDynamic = reader.ReadUInt16();
			LightmapST.Read(reader);
			LightmapSTDynamic.Read(reader);
			if (HasExplicitProbeSetHash(reader.Version))
			{
				ExplicitProbeSetHash.Read(reader);
			}
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(UVMesh, UvMeshName);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(UvMeshName, UVMesh.ExportYaml(container));
			node.Add(TerrainDynamicUVSTName, TerrainDynamicUVST.ExportYaml(container));
			node.Add(TerrainChunkDynamicUVSTName, TerrainChunkDynamicUVST.ExportYaml(container));
			node.Add(LightmapIndexName, LightmapIndex);
			node.Add(LightmapIndexDynamicName, LightmapIndexDynamic);
			node.Add(LightmapSTName, LightmapST.ExportYaml(container));
			node.Add(LightmapSTDynamicName, LightmapSTDynamic.ExportYaml(container));
			if (HasExplicitProbeSetHash(container.ExportVersion))
			{
				node.Add(ExplicitProbeSetHashName, ExplicitProbeSetHash.ExportYaml(container));
			}
			return node;
		}

		public ushort LightmapIndex { get; set; }
		public ushort LightmapIndexDynamic { get; set; }

		public const string UvMeshName = "uvMesh";
		public const string TerrainDynamicUVSTName = "terrainDynamicUVST";
		public const string TerrainChunkDynamicUVSTName = "terrainChunkDynamicUVST";
		public const string LightmapIndexName = "lightmapIndex";
		public const string LightmapIndexDynamicName = "lightmapIndexDynamic";
		public const string LightmapSTName = "lightmapST";
		public const string LightmapSTDynamicName = "lightmapSTDynamic";
		public const string ExplicitProbeSetHashName = "explicitProbeSetHash";

		public PPtr<Mesh.Mesh> UVMesh = new();
		public Vector4f TerrainDynamicUVST = new();
		public Vector4f TerrainChunkDynamicUVST = new();
		public Vector4f LightmapST = new();
		public Vector4f LightmapSTDynamic = new();
		public Hash128 ExplicitProbeSetHash = new();
	}
}
