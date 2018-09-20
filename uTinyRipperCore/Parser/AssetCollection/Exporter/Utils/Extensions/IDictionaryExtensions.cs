using System.Collections.Generic;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.AssetExporters
{
	public static class IDictionaryExtensions
	{
		public static YAMLNode ExportYAML<T>(this IReadOnlyDictionary<string, T> _this, IExportContainer container)
			where T: IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				map.Add(kvp.Key, kvp.Value.ExportYAML(container));
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML<T>(this IReadOnlyDictionary<T, int> _this, IExportContainer container)
			where T : IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				YAMLNode key = kvp.Key.ExportYAML(container);
				if (key.NodeType == YAMLNodeType.Scalar)
				{
					map.Add(key, kvp.Value);
				}
				else
				{
					map.Add("first", key);
					map.Add("second", kvp.Value);
				}
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML<T>(this IReadOnlyDictionary<T, float> _this, IExportContainer container)
			where T: IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				YAMLNode key = kvp.Key.ExportYAML(container);
				if(key.NodeType == YAMLNodeType.Scalar)
				{
					map.Add(key, kvp.Value);
				}
				else
				{
					map.Add("first", key);
					map.Add("second", kvp.Value);
				}
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML<T1, T2>(this IReadOnlyDictionary<T1, T2> _this, IExportContainer container)
			where T1 : IYAMLExportable
			where T2 : IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				YAMLNode key = kvp.Key.ExportYAML(container);
				if (key.NodeType == YAMLNodeType.Scalar)
				{
					map.Add(key, kvp.Value.ExportYAML(container));
				}
				else
				{
					map.Add("first", key);
					map.Add("second", kvp.Value.ExportYAML(container));
				}
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAMLArrayPPtr<T1, T2>(this IReadOnlyDictionary<T1, T2[]> _this, IExportContainer container)
			where T1 : IYAMLExportable
			where T2 : IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				YAMLNode key = kvp.Key.ExportYAML(container);
				if (key.NodeType == YAMLNodeType.Scalar)
				{
					map.Add(key, kvp.Value.ExportYAML(container));
				}
				else
				{
					map.Add("first", key);
					map.Add("second", kvp.Value.ExportYAML(container));
				}
				node.Add(map);
			}
			return node;
		}
	}
}
