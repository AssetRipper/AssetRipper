using AssetRipper.Converters.Project;
using AssetRipper.Layout.Classes.GameObject;
using AssetRipper.Parser.Asset;
using AssetRipper.Parser.Classes.Misc;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.Parser.IO.Asset.Writer;
using AssetRipper.YAML;
using System.Collections.Generic;

namespace AssetRipper.Parser.Classes.GameObject
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
