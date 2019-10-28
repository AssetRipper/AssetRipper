using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes.LODGroups
{
	public struct LODRenderer : IAssetReadable, IYAMLExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			Renderer.Read(reader);
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Renderer, RendererName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(RendererName, Renderer.ExportYAML(container));
			return node;
		}

		public const string RendererName = "renderer";

		public PPtr<Renderer> Renderer;
	}
}
