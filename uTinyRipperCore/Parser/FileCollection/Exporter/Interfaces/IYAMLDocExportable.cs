using uTinyRipper.YAML;

namespace uTinyRipper.AssetExporters
{
	public interface IYAMLDocExportable
	{
		YAMLDocument ExportYAMLDocument(IExportContainer container);
	}
}
