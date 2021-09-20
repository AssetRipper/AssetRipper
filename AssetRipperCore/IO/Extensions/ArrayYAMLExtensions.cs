using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.IO.Extensions
{
	public static class ArrayYAMLExtensions
	{
		public static YAMLNode ExportYAML<T>(this T[][] _this, IExportContainer container) where T : IYAMLExportable
		{
			return ((IEnumerable<IEnumerable<T>>)_this).ExportYAML(container);
		}
	}
}
