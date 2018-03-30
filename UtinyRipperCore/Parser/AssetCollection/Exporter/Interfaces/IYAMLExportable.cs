using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.AssetExporters
{
	public interface IYAMLExportable
	{
		YAMLNode ExportYAML(IAssetsExporter exporter);
	}
}
