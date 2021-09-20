using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout.Classes.GameObject;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.GameObject
{
	public struct ComponentPair : IAsset, IDependent
	{
		public void Read(AssetReader reader)
		{
			Component.Read(reader);
		}

		public void Write(AssetWriter writer)
		{
			Component.Write(writer);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			ComponentPairLayout layout = container.Layout.GameObject.ComponentPair;
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(layout.ComponentName, Component.ExportYAML(container));
			return node;
		}

		public IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			ComponentPairLayout layout = context.Layout.GameObject.ComponentPair;
			yield return context.FetchDependency(Component, layout.ComponentName);
		}

		public PPtr<Component> Component;
	}
}
