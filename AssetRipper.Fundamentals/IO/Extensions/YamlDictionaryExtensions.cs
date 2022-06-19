using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.IO.Extensions
{
	public static class YamlDictionaryExtensions
	{
		public static YamlNode ExportYaml<T>(this IReadOnlyDictionary<int, T> _this, IExportContainer container) where T : IYamlExportable
		{
			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.BlockCurve);
			foreach (KeyValuePair<int, T> kvp in _this)
			{
				YamlMappingNode map = new YamlMappingNode();
				map.Add(kvp.Key, kvp.Value.ExportYaml(container));
				node.Add(map);
			}
			return node;
		}

		public static YamlNode ExportYaml<T>(this IReadOnlyDictionary<string, T> _this, IExportContainer container) where T : IYamlExportable
		{
			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.BlockCurve);
			foreach (KeyValuePair<string, T> kvp in _this)
			{
				YamlMappingNode map = new YamlMappingNode();
				map.Add(kvp.Key, kvp.Value.ExportYaml(container));
				node.Add(map);
			}
			return node;
		}

		public static YamlNode ExportYaml<T1, T2>(this IReadOnlyDictionary<Tuple<T1, long>, T2> _this, IExportContainer container) where T1 : IYamlExportable
			where T2 : IYamlExportable
		{
#warning TODO: test
			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.BlockCurve);
			foreach (KeyValuePair<Tuple<T1, long>, T2> kvp in _this)
			{
				YamlMappingNode kvpMap = new YamlMappingNode();
				YamlMappingNode keyMap = new YamlMappingNode();
				keyMap.Add("first", kvp.Key.Item1.ExportYaml(container));
				keyMap.Add("second", kvp.Key.Item2);
				kvpMap.Add("first", keyMap);
				kvpMap.Add("second", kvp.Value.ExportYaml(container));
				node.Add(kvpMap);
			}
			return node;
		}

		public static YamlNode ExportYaml<T>(this IReadOnlyDictionary<T, int> _this, IExportContainer container) where T : IYamlExportable
		{
			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.BlockCurve);
			foreach (KeyValuePair<T, int> kvp in _this)
			{
				YamlMappingNode map = new YamlMappingNode();
				YamlNode key = kvp.Key.ExportYaml(container);
				if (key.NodeType == YamlNodeType.Scalar)
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

		public static YamlNode ExportYaml<T>(this IReadOnlyDictionary<T, float> _this, IExportContainer container) where T : IYamlExportable
		{
			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.BlockCurve);
			foreach (KeyValuePair<T, float> kvp in _this)
			{
				YamlMappingNode map = new YamlMappingNode();
				YamlNode key = kvp.Key.ExportYaml(container);
				if (key.NodeType == YamlNodeType.Scalar)
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

		public static YamlNode ExportYaml<T1, T2>(this IReadOnlyDictionary<T1, T2> _this, IExportContainer container) where T1 : IYamlExportable
			where T2 : IYamlExportable
		{
			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.BlockCurve);
			foreach (KeyValuePair<T1, T2> kvp in _this)
			{
				YamlMappingNode map = new YamlMappingNode();
				YamlNode key = kvp.Key.ExportYaml(container);
				if (key.NodeType == YamlNodeType.Scalar)
				{
					map.Add(key, kvp.Value.ExportYaml(container));
				}
				else
				{
					map.Add("first", key);
					map.Add("second", kvp.Value.ExportYaml(container));
				}
				node.Add(map);
			}
			return node;
		}

		public static YamlNode ExportYaml<T1, T2>(this IReadOnlyDictionary<T1, T2[]> _this, IExportContainer container) where T1 : IYamlExportable
			where T2 : IYamlExportable
		{
			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.BlockCurve);
			foreach (KeyValuePair<T1, T2[]> kvp in _this)
			{
				YamlMappingNode map = new YamlMappingNode();
				YamlNode key = kvp.Key.ExportYaml(container);
				if (key.NodeType == YamlNodeType.Scalar)
				{
					map.Add(key, kvp.Value.ExportYaml(container));
				}
				else
				{
					map.Add("first", key);
					map.Add("second", kvp.Value.ExportYaml(container));
				}
				node.Add(map);
			}
			return node;
		}
	}
}
