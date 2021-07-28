using AssetRipper.Project;
using AssetRipper.Parser.Asset;
using AssetRipper.Classes.Misc;
using AssetRipper.IO.Asset;
using AssetRipper.IO.Extensions;
using AssetRipper.YAML;
using System.Collections.Generic;

namespace AssetRipper.Classes.ResourceManager
{
	public struct ResourceManagerDependency : IAssetReadable, IYAMLExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			Object.Read(reader);
			Dependencies = reader.ReadAssetArray<PPtr<Object.Object>>();
		}

		public IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Object, ObjectName);
			foreach (PPtr<Object.Object> asset in context.FetchDependencies(Dependencies, DependenciesName))
			{
				yield return asset;
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(ObjectName, Object.ExportYAML(container));
			node.Add(DependenciesName, Dependencies.ExportYAML(container));
			return node;
		}

		public PPtr<Object.Object>[] Dependencies { get; set; }

		public const string ObjectName = "m_Object";
		public const string DependenciesName = "m_Dependencies";

		public PPtr<Object.Object> Object;
	}
}
