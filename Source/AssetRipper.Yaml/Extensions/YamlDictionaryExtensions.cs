namespace AssetRipper.Yaml.Extensions
{
	public static class YamlDictionaryExtensions
	{
		public static YamlNode ExportYaml(this IReadOnlyDictionary<uint, string> _this)
		{
			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.BlockCurve);
			foreach (KeyValuePair<uint, string> kvp in _this)
			{
				YamlMappingNode map = new YamlMappingNode(MappingStyle.Block);
				map.Add(kvp.Key, kvp.Value);
				node.Add(map);
			}
			return node;
		}

		public static YamlNode ExportYaml(this IReadOnlyDictionary<long, string> _this)
		{
			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.BlockCurve);
			foreach (KeyValuePair<long, string> kvp in _this)
			{
				YamlMappingNode map = new YamlMappingNode(MappingStyle.Block);
				map.Add(kvp.Key, kvp.Value);
				node.Add(map);
			}
			return node;
		}

		public static YamlNode ExportYaml(this IReadOnlyDictionary<string, string> _this)
		{
			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.BlockCurve);
			foreach (KeyValuePair<string, string> kvp in _this)
			{
				YamlMappingNode map = new YamlMappingNode(MappingStyle.Block);
				map.Add(kvp.Key, kvp.Value);
				node.Add(map);
			}
			return node;
		}

		public static YamlNode ExportYaml(this IReadOnlyDictionary<string, int> _this)
		{
			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.BlockCurve);
			foreach (KeyValuePair<string, int> kvp in _this)
			{
				YamlMappingNode map = new YamlMappingNode(MappingStyle.Block);
				map.Add(kvp.Key, kvp.Value);
				node.Add(map);
			}
			return node;
		}

		public static YamlNode ExportYaml(this IReadOnlyDictionary<string, float> _this)
		{
			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.BlockCurve);
			foreach (KeyValuePair<string, float> kvp in _this)
			{
				YamlMappingNode map = new YamlMappingNode(MappingStyle.Block);
				map.Add(kvp.Key, kvp.Value);
				node.Add(map);
			}
			return node;
		}

		public static YamlNode ExportYaml(this IReadOnlyDictionary<Tuple<ushort, ushort>, float> _this)
		{
			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.BlockCurve);
			foreach (KeyValuePair<Tuple<ushort, ushort>, float> kvp in _this)
			{
				YamlMappingNode keyNode = new YamlMappingNode();
				keyNode.Add(kvp.Key.Item1, kvp.Key.Item2);
				YamlMappingNode kvpMap = new YamlMappingNode();
				kvpMap.Add("first", keyNode);
				kvpMap.Add("second", kvp.Value);
				node.Add(kvpMap);
			}
			return node;
		}

		public static YamlNode ExportYaml(this IReadOnlyDictionary<Tuple<int, long>, string> _this)
		{
			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.BlockCurve);
			foreach (KeyValuePair<Tuple<int, long>, string> kvp in _this)
			{
				YamlMappingNode keyNode = new YamlMappingNode();
				keyNode.Add(kvp.Key.Item1, kvp.Key.Item2);
				YamlMappingNode kvpMap = new YamlMappingNode();
				kvpMap.Add("first", keyNode);
				kvpMap.Add("second", kvp.Value);
				node.Add(kvpMap);
			}
			return node;
		}

		public static YamlNode ExportYaml<T>(this IReadOnlyDictionary<Tuple<T, long>, string> _this, Func<T, int> converter)
		{
			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.BlockCurve);
			foreach (KeyValuePair<Tuple<T, long>, string> kvp in _this)
			{
				YamlMappingNode keyNode = new YamlMappingNode();
				keyNode.Add(converter(kvp.Key.Item1), kvp.Key.Item2);
				YamlMappingNode kvpMap = new YamlMappingNode();
				kvpMap.Add("first", keyNode);
				kvpMap.Add("second", kvp.Value);
				node.Add(kvpMap);
			}
			return node;
		}
	}
}
