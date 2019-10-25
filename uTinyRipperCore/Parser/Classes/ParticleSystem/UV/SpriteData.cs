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
		
		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Sprite.FetchDependency(file, isLog, () => nameof(SpriteData), "sprite");
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("sprite", Sprite.ExportYAML(container));
			return node;
		}

		public PPtr<Object> Sprite;
	}
}
