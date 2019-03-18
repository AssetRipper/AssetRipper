using uTinyRipper.YAML;

namespace uTinyRipper.AssetExporters
{
	public interface IYAMLExportable
	{
		YAMLNode ExportYAML(IExportContainer container);
	}
}
