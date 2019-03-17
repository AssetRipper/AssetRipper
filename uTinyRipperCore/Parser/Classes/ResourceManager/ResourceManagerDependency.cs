using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.ResourceManagers
{
	public struct ResourceManagerDependency : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Object.Read(reader);
			m_dependencies = reader.ReadAssetArray<PPtr<Object>>();
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Object.FetchDependency(file, isLog, () => nameof(ResourceManagerDependency), ObjectName);
			foreach (PPtr<Object> dependencie in Dependencies)
			{
				yield return dependencie.FetchDependency(file, isLog, () => nameof(ResourceManagerDependency), DependenciesName);
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
