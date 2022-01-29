using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Meta.Importers.Texture
{
	public sealed class SecondarySpriteTexture : IAsset, IDependent
	{
		public void Read(AssetReader reader)
		{
			Texture.Read(reader);
			Name = reader.ReadString();
			reader.AlignStream();

		}

		public void Write(AssetWriter writer)
		{
			Texture.Write(writer);
			writer.Write(Name);
			writer.AlignStream();

		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
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

		public PPtr<Texture2D.Texture2D> Texture = new();
	}
}
