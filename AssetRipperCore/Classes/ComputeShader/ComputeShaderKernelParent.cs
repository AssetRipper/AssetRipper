using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class ComputeShaderKernelParent : IAssetReadable, IYAMLExportable
	{
		public static bool HasSplitedKeywords(UnityVersion version) => version.IsGreaterEqual(2020, 2, 0, UnityVersionType.Alpha, 15);

		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			VariantMap = new Dictionary<string, ComputeShaderKernel>();
			VariantMap.Read(reader);
			if (HasSplitedKeywords(reader.Version))
			{
				GlobalKeywords = reader.ReadStringArray();
				LocalKeywords = reader.ReadStringArray();
			}
			else
			{
				ValidKeywords = reader.ReadStringArray();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("name", Name.ExportYAML());
			node.Add("variantMap", VariantMap.ExportYAML(container));
			if (HasSplitedKeywords(container.Version))
			{
				node.Add("globalKeywords", GlobalKeywords.ExportYAML());
				node.Add("localKeywords", LocalKeywords.ExportYAML());
			}
			else
			{
				node.Add("validKeywords", ValidKeywords.ExportYAML());
			}

			return node;
		}

		public string Name { get; set; }
		public Dictionary<string, ComputeShaderKernel> VariantMap { get; set; }
		public string[] GlobalKeywords { get; set; }
		public string[] LocalKeywords { get; set; }
		public string[] ValidKeywords { get; set; }
	}
}
