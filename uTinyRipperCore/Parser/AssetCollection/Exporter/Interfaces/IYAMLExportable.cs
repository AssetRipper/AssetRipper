using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.AssetExporters
{
	public interface IYAMLExportable
	{
		YAMLNode ExportYAML(IExportContainer container);
	}
}
