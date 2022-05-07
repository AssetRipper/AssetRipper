using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.ResourceManager
{
	public sealed class ResourceManagerDependency : IAssetReadable, IYamlExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			Object.Read(reader);
			Dependencies = reader.ReadAssetArray<PPtr<Object.Object>>();
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Object, ObjectName);
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(Dependencies, DependenciesName))
			{
				yield return asset;
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(ObjectName, Object.ExportYaml(container));
			node.Add(DependenciesName, Dependencies.ExportYaml(container));
			return node;
		}

		public PPtr<Object.Object>[] Dependencies { get; set; }

		public const string ObjectName = "m_Object";
		public const string DependenciesName = "m_Dependencies";

		public PPtr<Object.Object> Object = new();
	}
}
