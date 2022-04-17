using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.GameObject
{
	public sealed class ComponentPair : IAsset, IDependent
	{
		public void Read(AssetReader reader)
		{
			Component.Read(reader);
		}

		public void Write(AssetWriter writer)
		{
			Component.Write(writer);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(ComponentName, Component.ExportYaml(container));
			return node;
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Component, ComponentName);
		}

		public PPtr<Component> Component = new();
		public const string ComponentName = "component";
	}
}
