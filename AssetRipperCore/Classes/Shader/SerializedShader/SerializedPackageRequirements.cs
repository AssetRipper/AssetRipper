using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedPackageRequirements : IYAMLExportable
	{
		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Requirements", Array.Empty<string>().ExportYAML()); // As we have an empty array we don't need to use SerializedPackageRequirement
			node.Add("m_StatusMessage", default(string));
			node.Add("m_CombinedStatus", default(sbyte));
			return node;
		}
	}
}
