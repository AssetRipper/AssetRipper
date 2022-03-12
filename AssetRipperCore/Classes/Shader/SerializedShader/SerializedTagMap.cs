using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedTagMap : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_tags = new Dictionary<string, string>();
			m_tags.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("tags", m_tags.ExportYAML());
			return node;
		}

		public IReadOnlyDictionary<string, string> Tags => m_tags;
		private Dictionary<string, string> m_tags;
	}
}
