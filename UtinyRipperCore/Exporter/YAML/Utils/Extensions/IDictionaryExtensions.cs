using System.Collections.Generic;
using UtinyRipper.AssetExporters;

namespace UtinyRipper.Exporter.YAML
{
	public static class IDictionaryExtensions
	{
		public static YAMLNode ExportYAML(this IReadOnlyDictionary<uint, string> _this)
		{
#warning TODO: check
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				map.Add(kvp.Key.ToString(), kvp.Value);
				node.Add(map);
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

		public static YAMLNode ExportYAML(this IReadOnlyDictionary<string, float> _this)
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
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
