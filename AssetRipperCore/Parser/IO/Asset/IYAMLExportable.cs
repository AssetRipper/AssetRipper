using AssetRipper.Converters;
using AssetRipper.YAML;

namespace AssetRipper
{
	public interface IYAMLExportable
	{
		YAMLNode ExportYAML(IExportContainer container);
	}
}
