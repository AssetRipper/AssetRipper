using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Interfaces
{
	public interface IYAMLExportableNew : IYAMLExportable
	{
		YAMLNode ExportYAMLRelease();
		YAMLNode ExportYAMLDebug();
		YAMLNode ExportYAML(bool release);
	}
}
