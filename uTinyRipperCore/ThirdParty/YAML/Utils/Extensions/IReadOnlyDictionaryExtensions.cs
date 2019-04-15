using System.Collections.Generic;

namespace uTinyRipper.YAML
{
	public static class IReadOnlyDictionaryExtensions
	{
		public static YAMLNode ExportYAML(this IReadOnlyDictionary<uint, string> _this)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			foreach (var kvp in _this)
			{
				node.Add(kvp.Key, kvp.Value);
			}
			return node;
		}

		public static YAMLNode ExportYAML(this IReadOnlyDictionary<string, string> _this)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			foreach (var kvp in _this)
			{
				node.Add(kvp.Key, kvp.Value);
			}
			return node;
		}

		public static YAMLNode ExportYAML(this IReadOnlyDictionary<string, int> _this)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			foreach (var kvp in _this)
			{
				node.Add(kvp.Key, kvp.Value);
			}
			return node;
		}

		public static YAMLNode ExportYAML(this IReadOnlyDictionary<string, float> _this)
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				map.Add(kvp.Key, kvp.Value);
				node.Add(map);
			}
			return node;
		}
	}
}
