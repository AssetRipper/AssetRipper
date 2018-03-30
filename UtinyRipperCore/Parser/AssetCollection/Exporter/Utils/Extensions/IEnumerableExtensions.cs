using System.Collections.Generic;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.AssetExporters
{
	public static class IEnumerableExtensions
	{
		public static YAMLNode ExportYAML<T>(this IEnumerable<T> _this, IAssetsExporter exporter)
			where T: IYAMLExportable
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (T export in _this)
			{
				node.Add(export.ExportYAML(exporter));
			}
			return node;
		}
	}
}
