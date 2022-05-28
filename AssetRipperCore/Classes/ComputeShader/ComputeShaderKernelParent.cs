using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;

using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class ComputeShaderKernelParent : IAssetReadable, IYamlExportable
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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("name", Name.ExportYaml());
			node.Add("variantMap", VariantMap.ExportYaml(container));
			if (HasSplitedKeywords(container.Version))
			{
				node.Add("globalKeywords", GlobalKeywords.ExportYaml());
				node.Add("localKeywords", LocalKeywords.ExportYaml());
			}
			else
			{
				node.Add("validKeywords", ValidKeywords.ExportYaml());
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
