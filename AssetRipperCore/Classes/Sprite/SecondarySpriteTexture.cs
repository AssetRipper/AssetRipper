using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Sprite
{
	public sealed class SecondarySpriteTexture : IAssetReadable, IYamlExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			Texture.Read(reader);
			Name = reader.ReadString();
			reader.AlignStream();
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Texture, TextureName);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(TextureName, Texture.ExportYaml(container));
			node.Add(NameName, Name);
			return node;
		}

		public string Name { get; set; }

		public const string TextureName = "texture";
		public const string NameName = "name";

		public PPtr<Texture2D.Texture2D> Texture = new();
	}
}
