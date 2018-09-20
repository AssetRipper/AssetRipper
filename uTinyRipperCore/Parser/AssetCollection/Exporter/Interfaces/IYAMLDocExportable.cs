using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.AssetExporters
{
	public interface IYAMLDocExportable
	{
		YAMLDocument ExportYAMLDocument(IExportContainer container);
	}
}
