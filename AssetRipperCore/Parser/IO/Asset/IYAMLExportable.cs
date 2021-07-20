using AssetRipper.Converters.Project;
using AssetRipper.YAML;

namespace AssetRipper.Parser.IO.Asset
{
	public interface IYAMLExportable
	{
		YAMLNode ExportYAML(IExportContainer container);
	}
}
