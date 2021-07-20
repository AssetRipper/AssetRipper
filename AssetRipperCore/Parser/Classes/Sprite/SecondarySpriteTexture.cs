using AssetRipper.Converters.Project;
using AssetRipper.Parser.Asset;
using AssetRipper.Parser.Classes.Misc;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.YAML;
using System.Collections.Generic;

namespace AssetRipper.Parser.Classes.Sprite
{
	public struct SecondarySpriteTexture : IAssetReadable, IYAMLExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			Texture.Read(reader);
			Name = reader.ReadString();
			reader.AlignStream();
		}

		public IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Texture, TextureName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TextureName, Texture.ExportYAML(container));
			node.Add(NameName, Name);
			return node;
		}

		public string Name { get; set; }

		public const string TextureName = "texture";
		public const string NameName = "name";

		public PPtr<Texture2D.Texture2D> Texture;
	}
}
