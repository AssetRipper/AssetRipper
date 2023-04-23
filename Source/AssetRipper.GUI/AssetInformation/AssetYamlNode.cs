using AssetRipper.Yaml;

namespace AssetRipper.GUI.AssetInformation
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

		public AssetYamlNode(string key, YamlScalarNode value)
		{
			if (key == "_typelessdata")
			{
				//Potentially huge block of data, narrow it down
				DisplayName = $"{key}: <Byte Array of length {value.Value.Length / 2}>";
				return;
			}

			DisplayName = $"{key}: {value.Value}";

			if (string.IsNullOrEmpty(value.Value))
			{
				DisplayName += "<Null or Empty Value>";
			}
		}

		public AssetYamlNode(string key, YamlMappingNode value)
		{
			DisplayName = key;
			foreach ((YamlNode keyNode, YamlNode valueNode) in value.m_children)
			{
				string subKey = keyNode is YamlScalarNode s ? s.Value : keyNode.ToString()!;

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

		public AssetYamlNode(string key, YamlSequenceNode value)
		{
			DisplayName = key;
			int i = 0;
			foreach (YamlNode valueNode in value.Children)
			{
				string subKey = $"[{i}]";

				AppendChild(subKey, valueNode);

				i++;
			}

			if (value.Children.Count == 0)
			{
				DisplayName += ": <Empty Array>";
			}
			else
			{
				DisplayName += $" (Array of size {value.Children.Count})";
			}
		}

		private void AppendChild(string subKey, YamlNode valueNode)
		{
			AssetYamlNode child;
			if (valueNode is YamlScalarNode scalarNode)
			{
				child = new AssetYamlNode(subKey, scalarNode);
			}
			else if (valueNode is YamlMappingNode mappingNode)
			{
				child = new AssetYamlNode(subKey, mappingNode);
			}
			else if (valueNode is YamlSequenceNode sequenceNode)
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