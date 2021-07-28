using AssetRipper.Converters.Cubemap;
using AssetRipper.Project;
using AssetRipper.Parser.Asset;
using AssetRipper.Classes.Meta.Importers.Texture;
using AssetRipper.Classes.Misc;
using AssetRipper.Parser.Files;
using AssetRipper.IO.Asset;
using AssetRipper.IO.Extensions;
using AssetRipper.YAML;
using System.Collections.Generic;

namespace AssetRipper.Classes
{
	/// <summary>
	/// CubemapTexture previously
	/// </summary>
	public sealed class Cubemap : Texture2D.Texture2D
	{
		public Cubemap(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasSourceTextures(Version version) => version.IsGreaterEqual(4);

		public override TextureImporter GenerateTextureImporter(IExportContainer container)
		{
			return CubemapConverter.GeenrateTextureImporter(container, this);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasSourceTextures(reader.Version))
			{
				SourceTextures = reader.ReadAssetArray<PPtr<Texture2D.Texture2D>>();
				reader.AlignStream();
			}
		}

		public override IEnumerable<PPtr<Object.UnityObject>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object.UnityObject> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			if (HasSourceTextures(context.Version))
			{
				foreach (PPtr<Object.UnityObject> asset in context.FetchDependencies(SourceTextures, SourceTexturesName))
				{
					yield return asset;
				}
			}
		}

		protected sealed override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(SourceTexturesName, SourceTextures.ExportYAML(container));
			return node;
		}

		public PPtr<Texture2D.Texture2D>[] SourceTextures { get; set; }

		public const string SourceTexturesName = "m_SourceTextures";
	}
}
