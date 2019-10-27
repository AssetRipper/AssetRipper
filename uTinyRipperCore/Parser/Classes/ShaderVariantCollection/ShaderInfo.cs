using System.Collections.Generic;
using uTinyRipper.Project;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ShaderVariantCollections
{
	public struct ShaderInfo : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_variants = reader.ReadAssetArray<VariantInfo>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(VariantsName, Variants.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<VariantInfo> Variants => m_variants;

		public const string VariantsName = "variants";

		/// <summary>
		/// It's a HashSet actually
		/// </summary>
		private VariantInfo[] m_variants;
	}
}
