using System.Collections.Generic;
using uTinyRipper.Project;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.TerrainDatas
{
	public struct TreeDatabase : IAssetReadable, IAssetWritable
	{
		public void Read(AssetReader reader)
		{
			TreeInstances = reader.ReadAssetArray<TreeInstance>();
			TreePrototypes = reader.ReadAssetArray<TreePrototype>();
		}

		public void Write(AssetWriter writer)
		{
			writer.WriteAssetArray(TreeInstances);
			writer.WriteAssetArray(TreePrototypes);
		}

		public void ExportYAML(IExportContainer container, YAMLMappingNode node)
		{
			node.Add(TreeInstancesName, TreeInstances.ExportYAML(container));
			node.Add(TreePrototypesName, TreePrototypes.ExportYAML(container));
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (TreePrototype prototype in TreePrototypes)
			{
				foreach (Object asset in prototype.FetchDependencies(file, isLog))
				{
					yield return asset;
				}
			}
		}

		public TreeInstance[] TreeInstances { get; set; }
		public TreePrototype[] TreePrototypes { get; set; }

		public const string TreeInstancesName = "m_TreeInstances";
		public const string TreePrototypesName = "m_TreePrototypes";
	}
}
