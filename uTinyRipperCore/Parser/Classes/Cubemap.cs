using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// CubemapTexture previously
	/// </summary>
	public sealed class Cubemap : Texture2D
	{
		public Cubemap(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadSourceTextures(Version version)
		{
			return version.IsGreaterEqual(4);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadSourceTextures(reader.Version))
			{
				m_sourceTextures = reader.ReadAssetArray<PPtr<Texture2D>>();
				reader.AlignStream(AlignType.Align4);
			}
		}

		public override IEnumerable<Object> FetchDependencies(IDependencyContext context)
		{
			foreach(Object asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			if (IsReadSourceTextures(context.Version))
			{
				foreach (Object asset in context.FetchDependencies(SourceTextures, SourceTexturesName))
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

		public IReadOnlyList<PPtr<Texture2D>> SourceTextures => m_sourceTextures;

		public const string SourceTexturesName = "m_SourceTextures";

		private PPtr<Texture2D>[] m_sourceTextures;
	}
}
