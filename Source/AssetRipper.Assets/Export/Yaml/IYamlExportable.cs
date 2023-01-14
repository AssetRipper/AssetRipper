using AssetRipper.Yaml;

namespace AssetRipper.Assets.Export.Yaml
{
	public interface IYamlExportable
	{
		YamlNode ExportYaml(IExportContainer collection);
	}
}
