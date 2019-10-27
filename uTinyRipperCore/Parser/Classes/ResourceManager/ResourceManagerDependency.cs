using System.Collections.Generic;
using uTinyRipper.Project;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes.ResourceManagers
{
	public struct ResourceManagerDependency : IAssetReadable, IYAMLExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			Object.Read(reader);
			m_dependencies = reader.ReadAssetArray<PPtr<Object>>();
		}

		public IEnumerable<Object> FetchDependencies(IDependencyContext context)
		{
			yield return context.FetchDependency(Object, ObjectName);
			foreach (Object asset in context.FetchDependencies(Dependencies, DependenciesName))
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

		public IReadOnlyList<PPtr<Object>> Dependencies => m_dependencies;

		public const string ObjectName = "m_Object";
		public const string DependenciesName = "m_Dependencies";

		public PPtr<Object> Object;

		private PPtr<Object>[] m_dependencies;
	}
}
