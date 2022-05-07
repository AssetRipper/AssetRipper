using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.LODGroup
{
	public sealed class LODRenderer : IAssetReadable, IYamlExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			Renderer.Read(reader);
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Renderer, RendererName);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(RendererName, Renderer.ExportYaml(container));
			return node;
		}

		public const string RendererName = "renderer";

		public PPtr<Renderer.Renderer> Renderer = new();
	}
}
