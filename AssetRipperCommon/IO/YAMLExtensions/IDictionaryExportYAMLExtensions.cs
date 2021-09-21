using AssetRipper.Core.Interfaces;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.IO.Extensions
{
	public static class IDictionaryExportYAMLExtensions
	{
		public static YAMLNode ExportYAML<T>(this IReadOnlyDictionary<int, T> _this, bool release) where T : IYAMLExportableNew
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				map.Add(kvp.Key, kvp.Value.ExportYAML(release));
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML<T>(this IReadOnlyDictionary<string, T> _this, bool release) where T : IYAMLExportableNew
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				map.Add(kvp.Key, kvp.Value.ExportYAML(release));
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML<T1, T2>(this IReadOnlyDictionary<Tuple<T1, long>, T2> _this, bool release) where T1 : IYAMLExportableNew
			where T2 : IYAMLExportableNew
		{
#warning TODO: test
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode kvpMap = new YAMLMappingNode();
				YAMLMappingNode keyMap = new YAMLMappingNode();
				keyMap.Add("first", kvp.Key.Item1.ExportYAML(release));
				keyMap.Add("second", kvp.Key.Item2);
				kvpMap.Add("first", keyMap);
				kvpMap.Add("second", kvp.Value.ExportYAML(release));
				node.Add(kvpMap);
			}
			return node;
		}

		public static YAMLNode ExportYAML<T>(this IReadOnlyDictionary<T, int> _this, bool release) where T : IYAMLExportableNew
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				YAMLNode key = kvp.Key.ExportYAML(release);
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

		public static YAMLNode ExportYAML<T>(this IReadOnlyDictionary<T, float> _this, bool release) where T : IYAMLExportableNew
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				YAMLNode key = kvp.Key.ExportYAML(release);
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

		public static YAMLNode ExportYAML<T1, T2>(this IReadOnlyDictionary<T1, T2> _this, bool release) where T1 : IYAMLExportableNew
			where T2 : IYAMLExportableNew
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				YAMLNode key = kvp.Key.ExportYAML(release);
				if (key.NodeType == YAMLNodeType.Scalar)
				{
					map.Add(key, kvp.Value.ExportYAML(release));
				}
				else
				{
					map.Add("first", key);
					map.Add("second", kvp.Value.ExportYAML(release));
				}
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML<T1, T2>(this IReadOnlyDictionary<T1, T2[]> _this, bool release) where T1 : IYAMLExportableNew
			where T2 : IYAMLExportableNew
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				YAMLNode key = kvp.Key.ExportYAML(release);
				if (key.NodeType == YAMLNodeType.Scalar)
				{
					map.Add(key, kvp.Value.ExportYAML(release));
				}
				else
				{
					map.Add("first", key);
					map.Add("second", kvp.Value.ExportYAML(release));
				}
				node.Add(map);
			}
			return node;
		}
	}
}
