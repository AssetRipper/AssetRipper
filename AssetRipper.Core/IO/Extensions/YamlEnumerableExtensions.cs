using AssetRipper.Assets.Export;
using AssetRipper.Assets.Export.Yaml;
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
	}
}
