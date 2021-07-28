using AssetRipper.Project;
using AssetRipper.YAML;

namespace AssetRipper.IO.Asset
{
	public interface IYAMLExportable
	{
		YAMLNode ExportYAML(IExportContainer container);
	}
}
