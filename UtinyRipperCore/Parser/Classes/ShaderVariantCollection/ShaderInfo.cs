using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ShaderVariantCollections
{
	public struct ShaderInfo : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_variants = reader.ReadArray<VariantInfo>();
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
