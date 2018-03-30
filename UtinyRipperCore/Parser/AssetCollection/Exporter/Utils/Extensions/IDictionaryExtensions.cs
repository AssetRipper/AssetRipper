using System.Collections.Generic;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.AssetExporters
{
	public static class IDictionaryExtensions
	{
		public static YAMLNode ExportYAML<T>(this IReadOnlyDictionary<string, T> _this, IAssetsExporter exporter)
			where T: IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				map.Add(kvp.Key, kvp.Value.ExportYAML(exporter));
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML<T>(this IReadOnlyDictionary<T, float> _this, IAssetsExporter exporter)
			where T: IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				YAMLNode key = kvp.Key.ExportYAML(exporter);
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

		public static YAMLNode ExportYAML<T1, T2>(this IReadOnlyDictionary<T1, T2> _this, IAssetsExporter exporter)
			where T1 : IYAMLExportable
			where T2 : IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				YAMLNode key = kvp.Key.ExportYAML(exporter);
				if (key.NodeType == YAMLNodeType.Scalar)
				{
					map.Add(key, kvp.Value.ExportYAML(exporter));
				}
				else
				{
					map.Add("first", key);
					map.Add("second", kvp.Value.ExportYAML(exporter));
				}
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAMLArrayPPtr<T1, T2>(this IReadOnlyDictionary<T1, T2[]> _this, IAssetsExporter exporter)
			where T1 : IYAMLExportable
			where T2 : IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				YAMLNode key = kvp.Key.ExportYAML(exporter);
				if (key.NodeType == YAMLNodeType.Scalar)
				{
					map.Add(key, kvp.Value.ExportYAML(exporter));
				}
				else
				{
					map.Add("first", key);
					map.Add("second", kvp.Value.ExportYAML(exporter));
				}
				node.Add(map);
			}
			return node;
		}
	}
}
