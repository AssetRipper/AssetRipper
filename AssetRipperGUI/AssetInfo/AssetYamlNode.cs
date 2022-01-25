using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.GUI.AssetInfo
{
	public class AssetYamlNode
	{
		//Nodes can be either key-value (YamlScalarNode) or object (YamlMappingNode)/array (YamlSequenceNode).
		//If key-value, display name is "key: value"
		//If object/array, display name is "key", nested children are array elements.

		//Read from UI
		public string DisplayName { get; }
		//Read from UI
		public List<AssetYamlNode> Children { get; } = new();

		public AssetYamlNode(string key, YAMLScalarNode value)
		{
			if (key == "_typelessdata")
			{
				//Potentially huge block of data, narrow it down
				DisplayName = $"{key}: <Byte Array of length {value.Value.Length / 2}>";
				return;
			}

			DisplayName = $"{key}: {value.Value}";

			if (string.IsNullOrEmpty(value.Value))
				DisplayName += "<Null or Empty Value>";
		}

		public AssetYamlNode(string key, YAMLMappingNode value)
		{
			DisplayName = key;
			foreach (var (keyNode, valueNode) in value.m_children)
			{
				string subKey = keyNode is YAMLScalarNode s ? s.Value : keyNode.ToString()!;

				AppendChild(subKey, valueNode);
			}

			if (value.m_children.Count == 0)
			{
				DisplayName += ": <Empty Dictionary>";
			}
			else
			{
				DisplayName += $" (Dictionary with {value.m_children.Count} key{(value.m_children.Count == 1 ? "" : "s")})";
			}
		}

		public AssetYamlNode(string key, YAMLSequenceNode value)
		{
			DisplayName = key;
			int i = 0;
			foreach (var valueNode in value.m_children)
			{
				string subKey = $"[{i}]";

				AppendChild(subKey, valueNode);

				i++;
			}

			if (value.m_children.Count == 0)
			{
				DisplayName += ": <Empty Array>";
			}
			else
			{
				DisplayName += $" (Array of size {value.m_children.Count})";
			}
		}

		private void AppendChild(string subKey, YAMLNode valueNode)
		{
			AssetYamlNode child;
			if (valueNode is YAMLScalarNode scalarNode)
			{
				child = new AssetYamlNode(subKey, scalarNode);
			}
			else if (valueNode is YAMLMappingNode mappingNode)
			{
				child = new AssetYamlNode(subKey, mappingNode);
			}
			else if (valueNode is YAMLSequenceNode sequenceNode)
			{
				child = new AssetYamlNode(subKey, sequenceNode);
			}
			else
			{
				return;
			}

			Children.Add(child);
		}
	}
}