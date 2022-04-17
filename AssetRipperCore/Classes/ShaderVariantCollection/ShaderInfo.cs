using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.ShaderVariantCollection
{
	public sealed class ShaderInfo : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Variants = reader.ReadAssetArray<VariantInfo>();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(VariantsName, Variants.ExportYaml(container));
			return node;
		}

		/// <summary>
		/// It's a HashSet actually
		/// </summary>
		public VariantInfo[] Variants { get; set; }

		public const string VariantsName = "variants";
	}
}
