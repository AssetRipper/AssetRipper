using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.IO.Extensions
{
	public static class YamlArrayExtensions
	{
		public static YamlNode ExportYaml<T>(this T[][] _this, IExportContainer container) where T : IYamlExportable
		{
			return ((IEnumerable<IEnumerable<T>>)_this).ExportYaml(container);
		}
	}
}
