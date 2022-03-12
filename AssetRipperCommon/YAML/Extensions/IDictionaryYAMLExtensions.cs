using System;
using System.Collections.Generic;

namespace AssetRipper.Core.YAML.Extensions
{
	public static class IDictionaryYAMLExtensions
	{
		public static YAMLNode ExportYAML(this IReadOnlyDictionary<uint, string> _this)
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode(MappingStyle.Block);
				map.Add(kvp.Key, kvp.Value);
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML(this IReadOnlyDictionary<long, string> _this)
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode(MappingStyle.Block);
				map.Add(kvp.Key, kvp.Value);
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML(this IReadOnlyDictionary<string, string> _this)
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode(MappingStyle.Block);
				map.Add(kvp.Key, kvp.Value);
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML(this IReadOnlyDictionary<string, int> _this)
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode(MappingStyle.Block);
				map.Add(kvp.Key, kvp.Value);
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML(this IReadOnlyDictionary<string, float> _this)
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode(MappingStyle.Block);
				map.Add(kvp.Key, kvp.Value);
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML(this IReadOnlyDictionary<Tuple<ushort, ushort>, float> _this)
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode keyNode = new YAMLMappingNode();
				keyNode.Add(kvp.Key.Item1, kvp.Key.Item2);
				YAMLMappingNode kvpMap = new YAMLMappingNode();
				kvpMap.Add("first", keyNode);
				kvpMap.Add("second", kvp.Value);
				node.Add(kvpMap);
			}
			return node;
		}

		public static YAMLNode ExportYAML(this IReadOnlyDictionary<Tuple<int, long>, string> _this)
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode keyNode = new YAMLMappingNode();
				keyNode.Add(kvp.Key.Item1, kvp.Key.Item2);
				YAMLMappingNode kvpMap = new YAMLMappingNode();
				kvpMap.Add("first", keyNode);
				kvpMap.Add("second", kvp.Value);
				node.Add(kvpMap);
			}
			return node;
		}

		public static YAMLNode ExportYAML<T>(this IReadOnlyDictionary<Tuple<T, long>, string> _this, Func<T, int> converter)
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.BlockCurve);
			foreach (var kvp in _this)
			{
				YAMLMappingNode keyNode = new YAMLMappingNode();
				keyNode.Add(converter(kvp.Key.Item1), kvp.Key.Item2);
				YAMLMappingNode kvpMap = new YAMLMappingNode();
				kvpMap.Add("first", keyNode);
				kvpMap.Add("second", kvp.Value);
				node.Add(kvpMap);
			}
			return node;
		}
	}
}
