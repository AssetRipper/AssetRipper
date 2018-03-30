using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.AssetExporters
{
	public interface IYAMLDocExportable
	{
		YAMLDocument ExportYAMLDocument(IAssetsExporter exporter);
	}
}
