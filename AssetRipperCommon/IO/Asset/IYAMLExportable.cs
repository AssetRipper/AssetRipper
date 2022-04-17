using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.IO.Asset
{
	public interface IYamlExportable
	{
		YamlNode ExportYaml(IExportContainer container);
	}
}
