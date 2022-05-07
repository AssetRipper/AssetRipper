using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;

using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedTagMap : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			m_tags = new Dictionary<string, string>();
			m_tags.Read(reader);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("tags", m_tags.ExportYaml());
			return node;
		}

		public IReadOnlyDictionary<string, string> Tags => m_tags;
		private Dictionary<string, string> m_tags;
	}
}
