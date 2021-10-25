using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Interfaces
{
	public interface IYAMLExportableNew : IYAMLExportable
	{
		YAMLNode ExportYAMLRelease(IExportContainer container);
		YAMLNode ExportYAMLDebug(IExportContainer container);
	}
}
