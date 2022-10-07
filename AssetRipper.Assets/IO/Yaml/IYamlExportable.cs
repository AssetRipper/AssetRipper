using AssetRipper.Assets.Export;
using AssetRipper.Yaml;

namespace AssetRipper.Assets.IO.Yaml
{
	public interface IYamlExportable
	{
		YamlNode ExportYaml(IExportContainer collection);
	}
}
