using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.IO.Extensions
{
	public static class YamlEnumerableExtensions
	{
		public static YamlNode ExportYaml<T>(this IEnumerable<T> _this, IExportContainer container) where T : IYamlExportable
		{
			if (_this == null)
			{
				throw new ArgumentNullException(nameof(_this));
			}

			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.Block);
			foreach (T export in _this)
			{
				node.Add(export.ExportYaml(container));
			}
			return node;
		}

		public static YamlNode ExportYaml<T>(this IEnumerable<IEnumerable<T>> _this, IExportContainer container) where T : IYamlExportable
		{
			if (_this == null)
			{
				throw new ArgumentNullException(nameof(_this));
			}

			YamlSequenceNode node = new YamlSequenceNode(SequenceStyle.Block);
			foreach (IEnumerable<T> export in _this)
			{
				node.Add(export.ExportYaml(container));
			}
			return node;
		}

		public static YamlNode ExportYaml<T>(this IEnumerable<Tuple<string, T>> _this, IExportContainer container) where T : IYamlExportable
		{
			if (_this == null)
			{
				throw new ArgumentNullException(nameof(_this));
			}

			YamlSequenceNode node = new YamlSequenceNode();
			foreach (Tuple<string, T> kvp in _this)
			{
				YamlMappingNode map = new YamlMappingNode();
				map.Add(kvp.Item1, kvp.Item2.ExportYaml(container));
				node.Add(map);
			}
			return node;
		}

		public static YamlNode ExportYaml<T1, T2>(this IEnumerable<Tuple<T1, T2>> _this, IExportContainer container, Func<T1, int> converter) where T2 : IYamlExportable
		{
			if (_this == null)
			{
				throw new ArgumentNullException(nameof(_this));
			}

			YamlSequenceNode node = new YamlSequenceNode();
			foreach (Tuple<T1, T2> kvp in _this)
			{
				YamlMappingNode map = new YamlMappingNode();
				map.Add(converter(kvp.Item1), kvp.Item2.ExportYaml(container));
				node.Add(map);
			}
			return node;
		}

		public static YamlNode ExportYaml<T>(this IEnumerable<KeyValuePair<string, T>> _this, IExportContainer container) where T : IYamlExportable
		{
			if (_this == null)
			{
				throw new ArgumentNullException(nameof(_this));
			}

			YamlSequenceNode node = new YamlSequenceNode();
			foreach (KeyValuePair<string, T> kvp in _this)
			{
				YamlMappingNode map = new YamlMappingNode();
				map.Add(kvp.Key, kvp.Value.ExportYaml(container));
				node.Add(map);
			}
			return node;
		}
	}
}
