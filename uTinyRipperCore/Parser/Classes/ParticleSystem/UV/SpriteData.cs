using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes.ParticleSystems
{
	public struct SpriteData : IAssetReadable, IYAMLExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			Sprite.Read(reader);
		}
		
		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Sprite, SpriteName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(SpriteName, Sprite.ExportYAML(container));
			return node;
		}

		public const string SpriteName = "sprite";

		public PPtr<Object> Sprite;
	}
}
