using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.IO.Asset
{
	public interface IYAMLExportable
	{
		YAMLNode ExportYAML(IExportContainer container);
	}
}
