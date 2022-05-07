using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.ParticleSystem.UV
{
	public sealed class SpriteData : IAssetReadable, IYamlExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			Sprite.Read(reader);
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Sprite, SpriteName);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(SpriteName, Sprite.ExportYaml(container));
			return node;
		}

		public const string SpriteName = "sprite";

		public PPtr<Object.Object> Sprite = new();
	}
}
