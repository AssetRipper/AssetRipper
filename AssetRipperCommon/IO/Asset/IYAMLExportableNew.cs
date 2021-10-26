using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.IO.Asset
{
	public interface IYAMLExportableNew : IYAMLExportable
	{
		YAMLNode ExportYAMLRelease(IExportContainer container);
		YAMLNode ExportYAMLDebug(IExportContainer container);
	}
}
