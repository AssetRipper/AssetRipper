using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Interfaces
{
	public interface IYAMLExportableNew
	{
		YAMLNode ExportYAMLRelease();
		YAMLNode ExportYAMLDebug();
		YAMLNode ExportYAML(bool release);
	}
}
