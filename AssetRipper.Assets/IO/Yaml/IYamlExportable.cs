using AssetRipper.Assets.Collections;
using AssetRipper.Yaml;

namespace AssetRipper.Assets.IO.Yaml
{
	public interface IYamlExportable
	{
		YamlNode ExportYaml(AssetCollection collection);
	}
}
