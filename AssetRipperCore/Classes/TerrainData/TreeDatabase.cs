﻿using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.TerrainData
{
	public sealed class TreeDatabase : IAssetReadable, IAssetWritable
	{
		public void Read(AssetReader reader)
		{
			TreeInstances = reader.ReadAssetArray<TreeInstance>();
			TreePrototypes = reader.ReadAssetArray<TreePrototype>();
		}

		public void Write(AssetWriter writer)
		{
			TreeInstances.Write(writer);
			TreePrototypes.Write(writer);
		}

		public void ExportYaml(IExportContainer container, YamlMappingNode node)
		{
			node.Add(TreeInstancesName, TreeInstances.ExportYaml(container));
			node.Add(TreePrototypesName, TreePrototypes.ExportYaml(container));
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromArray(TreePrototypes, TreeInstancesName))
			{
				yield return asset;
			}
		}

		public TreeInstance[] TreeInstances { get; set; }
		public TreePrototype[] TreePrototypes { get; set; }

		public const string TreeInstancesName = "m_TreeInstances";
		public const string TreePrototypesName = "m_TreePrototypes";
	}
}
