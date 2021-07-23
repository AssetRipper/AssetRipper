using AssetRipper.Converters.Project;
using AssetRipper.IO.Asset;
using AssetRipper.YAML;
using System.Collections.Generic;

namespace AssetRipper.IO.Extensions
{
	public static class ArrayYAMLExtensions
	{
		public static YAMLNode ExportYAML<T>(this T[][] _this, IExportContainer container)
			where T : IYAMLExportable
		{
			return ((IEnumerable<IEnumerable<T>>)_this).ExportYAML(container);
		}
	}
}
