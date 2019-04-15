using System;
using System.Collections.Generic;
using uTinyRipper.YAML;

namespace uTinyRipper.AssetExporters
{
	public static class IDictionaryExtensions
	{
		public static YAMLNode ExportYAML<T>(this IReadOnlyDictionary<int, T> _this, IExportContainer container)
			where T : IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				map.Add(kvp.Key, kvp.Value.ExportYAML(container));
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML<T>(this IReadOnlyDictionary<string, T> _this, IExportContainer container)
			where T: IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				map.Add(kvp.Key, kvp.Value.ExportYAML(container));
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML<T1, T2>(this IReadOnlyDictionary<Tuple<T1, long>, T2> _this, IExportContainer container)
			where T1 : IYAMLExportable
			where T2 : IYAMLExportable
		{
			// TODO: test
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode kvpMap = new YAMLMappingNode();
				YAMLMappingNode keyMap = new YAMLMappingNode();
				keyMap.Add("first", kvp.Key.Item1.ExportYAML(container));
				keyMap.Add("second", kvp.Key.Item2);
				kvpMap.Add("first", keyMap);
				kvpMap.Add("second", kvp.Value.ExportYAML(container));
				node.Add(kvpMap);
			}
			return node;
		}

		public static YAMLNode ExportYAML(this IReadOnlyDictionary<Tuple<ushort, ushort>, float> _this, IExportContainer container)
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode kvpMap = new YAMLMappingNode();
				YAMLMappingNode keyMap = new YAMLMappingNode();
				keyMap.Add("first", kvp.Key.Item1);
				keyMap.Add("second", kvp.Key.Item2);
				kvpMap.Add("first", keyMap);
				kvpMap.Add("second", kvp.Value);
				node.Add(kvpMap);
			}
			return node;
		}

		public static YAMLNode ExportYAML<T>(this IReadOnlyDictionary<T, int> _this, IExportContainer container)
			where T : IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
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
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
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
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
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

		public static YAMLNode ExportYAML<T1, T2>(this IReadOnlyDictionary<T1, T2[]> _this, IExportContainer container)
			where T1 : IYAMLExportable
			where T2 : IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
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
