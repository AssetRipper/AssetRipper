using System.Collections.Generic;
using uTinyRipper.YAML;

namespace uTinyRipper.AssetExporters
{
	public static class IEnumerableExtensions
	{
		public static YAMLNode ExportYAML<T>(this IEnumerable<T> _this, IExportContainer container)
			where T: IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (T export in _this)
			{
				node.Add(export.ExportYAML(container));
			}
			return node;
		}

		public static YAMLNode ExportYAML<T>(this IEnumerable<IEnumerable<T>> _this, IExportContainer container)
			where T: IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (IEnumerable<T> export in _this)
			{
				node.Add(export.ExportYAML(container));
			}
			return node;
		}

		public static YAMLNode ExportYAML<T>(this IEnumerable<IReadOnlyList<T>> _this, IExportContainer container)
			where T : IYAMLExportable
		{
			return ExportYAML((IEnumerable<IEnumerable<T>>)_this, container);
		}

		public static YAMLNode ExportYAML<T>(this IEnumerable<KeyValuePair<string, T>> _this, IExportContainer container)
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
	}
}
