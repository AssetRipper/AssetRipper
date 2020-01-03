using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ShaderVariantCollections
{
	public struct ShaderInfo : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Variants = reader.ReadAssetArray<VariantInfo>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(VariantsName, Variants.ExportYAML(container));
			return node;
		}

		/// <summary>
		/// It's a HashSet actually
		/// </summary>
		public VariantInfo[] Variants { get; set; }

		public const string VariantsName = "variants";
	}
}
