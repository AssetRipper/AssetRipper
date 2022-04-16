using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.IO.Asset
{
	public interface IYAMLExportable
	{
		YAMLNode ExportYAML(IExportContainer container);
	}
}
