using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;

using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedPackageRequirements : IYamlExportable
	{
		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("m_Requirements", Array.Empty<string>().ExportYaml()); // As we have an empty array we don't need to use SerializedPackageRequirement
			node.Add("m_StatusMessage", default(string));
			node.Add("m_CombinedStatus", default(sbyte));
			return node;
		}
	}
}
