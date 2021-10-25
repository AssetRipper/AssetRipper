using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.IO.Extensions
{
	public static class IEnumerableYAMLExtensions
	{
		public static YAMLNode ExportYAML<T>(this IEnumerable<T> _this, IExportContainer container) where T : IYAMLExportable
		{
			if (_this == null) throw new ArgumentNullException(nameof(_this));

			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (T export in _this)
			{
				node.Add(export.ExportYAML(container));
			}
			return node;
		}

		public static YAMLNode ExportYAML<T>(this IEnumerable<IEnumerable<T>> _this, IExportContainer container) where T : IYAMLExportable
		{
			if (_this == null) throw new ArgumentNullException(nameof(_this));

			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (IEnumerable<T> export in _this)
			{
				node.Add(export.ExportYAML(container));
			}
			return node;
		}

		public static YAMLNode ExportYAML<T>(this IEnumerable<Tuple<string, T>> _this, IExportContainer container) where T : IYAMLExportable
		{
			if (_this == null) throw new ArgumentNullException(nameof(_this));

			YAMLSequenceNode node = new YAMLSequenceNode();
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				map.Add(kvp.Item1, kvp.Item2.ExportYAML(container));
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML<T1, T2>(this IEnumerable<Tuple<T1, T2>> _this, IExportContainer container, Func<T1, int> converter) where T2 : IYAMLExportable
		{
			if (_this == null) throw new ArgumentNullException(nameof(_this));

			YAMLSequenceNode node = new YAMLSequenceNode();
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				map.Add(converter(kvp.Item1), kvp.Item2.ExportYAML(container));
				node.Add(map);
			}
			return node;
		}

		public static YAMLNode ExportYAML<T>(this IEnumerable<KeyValuePair<string, T>> _this, IExportContainer container) where T : IYAMLExportable
		{
			if (_this == null) throw new ArgumentNullException(nameof(_this));

			YAMLSequenceNode node = new YAMLSequenceNode();
			foreach (var kvp in _this)
			{
				YAMLMappingNode map = new YAMLMappingNode();
				map.Add(kvp.Key, kvp.Value.ExportYAML(container));
				node.Add(map);
			}
			return node;
		}
	}
}
