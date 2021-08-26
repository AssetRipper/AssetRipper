using AssetRipper.Core.YAML;

namespace AssetRipper.Core.IO.Asset
{
	public interface IYAMLExportableNew
	{
		YAMLNode ExportYAMLRelease();
		YAMLNode ExportYAMLDebug();
		YAMLNode ExportYAML(bool release);
	}
}
