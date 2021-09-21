using AssetRipper.Core.Interfaces;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.IO.Extensions
{
	public static class ArrayYAMLExtensions
	{
		public static YAMLNode ExportYAML<T>(this T[][] _this, bool release) where T : IYAMLExportableNew
		{
			return ((IEnumerable<IEnumerable<T>>)_this).ExportYAML(release);
		}
	}
}
