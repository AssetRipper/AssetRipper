using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper
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
