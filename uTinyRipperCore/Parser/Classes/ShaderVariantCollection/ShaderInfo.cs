using System.Collections.Generic;
using uTinyRipper.AssetExporters;
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
			node.Add("variants", Variants.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<VariantInfo> Variants => m_variants;

		/// <summary>
		/// It's a HashSet actually
		/// </summary>
		private VariantInfo[] m_variants;
	}
}
