using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper
{
	public interface IYAMLExportable
	{
		YAMLNode ExportYAML(IExportContainer container);
	}
}
