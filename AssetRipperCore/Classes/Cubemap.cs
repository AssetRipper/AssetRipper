using AssetRipper.Core.Classes.Meta.Importers.Texture;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Converters.Cubemap;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes
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
		public static bool HasSourceTextures(UnityVersion version) => version.IsGreaterEqual(4);

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

		public override IEnumerable<PPtr<UnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<UnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			if (HasSourceTextures(context.Version))
			{
				foreach (PPtr<UnityObjectBase> asset in context.FetchDependencies(SourceTextures, SourceTexturesName))
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
